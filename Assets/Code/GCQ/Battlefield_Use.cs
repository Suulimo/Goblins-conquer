using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using DG.Tweening;
using System.Collections.Generic;
using UniRx;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Tilemaps;
// 縮短使用命名空間
using static GCQ.Static_Game_Scope;

namespace GCQ
{
    public class Battlefield_Use
    {
        public enum Cursor_Mode {
            Drag,
            Select,
            Cast,
        }

        Cursor_Mode cursor_mode = Cursor_Mode.Drag;

        public Cursor_Mode current_cursor_mode => cursor_mode;

        Battlefield_Main_Monobe main_component;

        bool is_dragging;
        bool is_drag_ready;

        Slot_Type to_slot_type;
        int2 to_slot_id;

        Vector3 out_of_screen = new Vector3(-10000, 0, 0);

        Dictionary<(Slot_Type, int2), Battlefield_Slot_Monobe> slot_look_up;
        //Dictionary<Slot_Type, Battlefield_Slot_Control[]> slot_type_to_group;

        Collider2D[] overlapResults = new Collider2D[50];
        const float movetime = .15f;

        Camera camera_main = null;

        Vector3Int[] test_cast_shape = { new(0, 0, 0), new(-1, 0, 0), new(0, 1, 0), new(1, 0, 0), new(0, -1, 0) };

        public Battlefield_Slot_Monobe Get_Slot_Monobe(Slot_Type type, int2 id) {
            return slot_look_up[(type, id)];
        }


        public Battlefield_Use(Battlefield_Main_Monobe battlefield_main_component) {
            main_component = battlefield_main_component;

            MessageBroker.Default.Receive<Drag_To>().Subscribe(OnDragTo).AddTo(main_component);
            MessageBroker.Default.Receive<Drag_To_Cancel>().Subscribe(OnDragToCancel).AddTo(main_component);
            MessageBroker.Default.Receive<Drag_Begin>().Subscribe(OnDragBegin).AddTo(main_component);
            MessageBroker.Default.Receive<Drag_End>().Subscribe(OnDragEnd).AddTo(main_component);
            MessageBroker.Default.Receive<Drag_Cancel>().Subscribe(OnDragCancel).AddTo(main_component);

            MessageBroker.Default.Receive<Human_Spawned>().Subscribe(Battlefield_Execution.On_Spawn_Human).AddTo(main_component);
            MessageBroker.Default.Receive<Goblin_Spawned>().Subscribe(Battlefield_Execution.On_Spawn_Goblin).AddTo(main_component);
            MessageBroker.Default.Receive<Bed_Spawned>().Subscribe(Battlefield_Execution.On_Spawn_Bed).AddTo(main_component);

            slot_look_up = new Dictionary<(Slot_Type, int2), Battlefield_Slot_Monobe>();
        }

        public void Start_My_Update() {
            UniTaskAsyncEnumerable.EveryUpdate().Subscribe(_ => {
                My_Update(Time.deltaTime);
            }).AddTo(main_component);
        }

        void My_Update(float dt) {

            if (cursor_mode == Cursor_Mode.Drag) {
                if (is_dragging) {
                    var cursor = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                    cursor.z = 0;
                    main_component.dragged.transform.position = cursor;
                }
            }
            else if (cursor_mode == Cursor_Mode.Cast) {

                camera_main ??= Camera.main;

                Reset_Tile_State();

                var tilemap = main_component.tilemap;
                var wPos = camera_main.ScreenToWorldPoint(Input.mousePosition);

                var mouse_in_cell = tilemap.WorldToCell(wPos);
                for (int i = 0; i < test_cast_shape.Length; i++) {
                    var shape_i = test_cast_shape[i];
                    var shape_cell = mouse_in_cell + shape_i;

                    if (tiles_bounds_a.Contains(shape_cell))
                        tiles_info_a[shape_cell.x - tiles_base_a.x][shape_cell.y - tiles_base_a.y]?.Set_Mark_Color();
                }
            }
        }

        public void Change_Cursor_Mode(Cursor_Mode mode) {
            cursor_mode = mode;

            Reset_Tile_State();
        }

        public void Test_Hp_Item_Cast() {
            if (cursor_mode == Cursor_Mode.Cast) {

                camera_main ??= Camera.main;

                Reset_Tile_State();

                var tilemap = main_component.tilemap;
                var wPos = camera_main.ScreenToWorldPoint(Input.mousePosition);

                var mouse_in_cell = tilemap.WorldToCell(wPos);
                Debug.Log(mouse_in_cell);
                for (int i = 0; i < test_cast_shape.Length; i++) {
                    var shape_i = test_cast_shape[i];
                    var shape_cell = mouse_in_cell + shape_i;

                    if (tiles_bounds_a.Contains(shape_cell)) {
                        var slot_data = tiles_info_a[shape_cell.x - tiles_base_a.x][shape_cell.y - tiles_base_a.y].Data;
                        if (slot_data.goblin != null) {
                            slot_data.goblin.combat.hp.Value = Mathf.Min(slot_data.goblin.combat.hp.Value + 50, slot_data.goblin.combat.hp_max);
                        }
                    }
                        
                }
            }
        }

        void OnDragTo(Drag_To args) {
            if (is_dragging == false)
                return;

            to_slot_type = args.slot_type;
            to_slot_id = args.slot_id;

            is_drag_ready = true;
        }

        void OnDragToCancel(Drag_To_Cancel _) {
            if (is_dragging == false)
                return;
            is_drag_ready = false;
        }

        void OnDragBegin(Drag_Begin args) {
            if (is_dragging)
                return;

            var slot = slot_look_up[(args.slot_type, args.slot_id)];
            if (slot == null)
                return;

            var info = slot.Get_Swap_Info;
            //main_component.dragged.color = slot.Get_Swap_Info.color;
            main_component.dragged.sprite = info.sprite;
            main_component.dragged.flipX = info.flip_x;

            is_dragging = true;
        }

        void OnDragEnd(Drag_End args) {

            main_component.dragged.transform.position = out_of_screen;


            if (is_dragging && is_drag_ready) {

                if (to_slot_type == Slot_Type.U) {
                    // 丟棄
                    var discard = slot_look_up[(args.slot_type, args.slot_id)];
                    if (discard != null) {
                        SpawnPrefabSystem.SafeReturn(discard.Get_Pawn_Object);
                        SpawnPrefabSystem.SafeReturn(discard.Get_Slot_Display_Object);
                        Battle_Sys.Discard_Pawn_At(Static_Game_Scope.battle_scope, args.slot_type, args.slot_id);
                    }

                }
                else {
                    // 交換或移動
                    var swap_a = slot_look_up[(to_slot_type, to_slot_id)];
                    var swap_b = slot_look_up[(args.slot_type, args.slot_id)];

                    if (swap_a && swap_b && (swap_a.slot_type == swap_b.slot_type)) {

                        bool busy_a = Battle_Sys.Ask_Slot_Is_Busy(swap_a.Data);
                        bool busy_b = Battle_Sys.Ask_Slot_Is_Busy(swap_b.Data);

                        if (to_slot_type == Slot_Type.C) {
                            if (!swap_a.Data.is_empty && !swap_b.Data.is_empty && !busy_a && !busy_b && swap_a.Data.bed != null && swap_b.Data.bed != null) {
                                var c = swap_a.Get_Swap_Info;
                                swap_a.Set_Swap_Info(swap_b.Get_Swap_Info);
                                swap_b.Set_Swap_Info(c);
                                _ = Tk_Swap_Pawn(swap_a, swap_b);
                            }
                            else if (swap_a.Data.is_empty && swap_b.Data.is_empty == false && swap_a.Data.bed != null) {
                                var c = swap_a.Get_Swap_Info;
                                swap_a.Set_Swap_Info(swap_b.Get_Swap_Info);
                                swap_b.Set_Swap_Info(c);
                                _ = Tk_Move_Pawn(swap_a, swap_b);
                            }
                        }
                        else {
                            // 交換
                            if (!swap_a.Data.is_empty && !swap_b.Data.is_empty && !busy_a && !busy_b) {
                                var c = swap_a.Get_Swap_Info;
                                swap_a.Set_Swap_Info(swap_b.Get_Swap_Info);
                                swap_b.Set_Swap_Info(c);

                                _ = Tk_Swap_Pawn(swap_a, swap_b);
                            }
                            // 移動
                            else if (swap_a.Data.is_empty && swap_b.Data.is_empty == false) {
                                var c = swap_a.Get_Swap_Info;
                                swap_a.Set_Swap_Info(swap_b.Get_Swap_Info);
                                swap_b.Set_Swap_Info(c);
                                _ = Tk_Move_Pawn(swap_a, swap_b);
                            }
                        }
                    }

                    // siu息
                    if (swap_a && swap_b && (to_slot_type, args.slot_type) == (Slot_Type.C, Slot_Type.A)) {

                        bool busy_a = Battle_Sys.Ask_Slot_Is_Busy(swap_a.Data);
                        bool busy_b = Battle_Sys.Ask_Slot_Is_Busy(swap_b.Data);

                        if (swap_a.Data.is_empty && swap_b.Data.is_empty == false && swap_a.Data.bed != null) {
                            _ = Tk_Goblin_Goto_Bed(swap_a, swap_b);
                        }
                        else if (!swap_a.Data.is_empty && !swap_b.Data.is_empty && !busy_a && !busy_b) {
                            var c = swap_a.Get_Swap_Info;
                            swap_a.Set_Swap_Info(swap_b.Get_Swap_Info);
                            swap_b.Set_Swap_Info(c);

                            _ = Tk_Swap_Pawn(swap_a, swap_b);
                        }

                    }
                    else if (swap_a && swap_b && (to_slot_type, args.slot_type) == (Slot_Type.A, Slot_Type.C)) {

                        bool busy_a = Battle_Sys.Ask_Slot_Is_Busy(swap_a.Data);
                        bool busy_b = Battle_Sys.Ask_Slot_Is_Busy(swap_b.Data);

                        if (swap_a.Data.is_empty && swap_b.Data.is_empty == false) {
                            _ = Tk_Goblin_Goto_Bed(swap_a, swap_b);
                        }
                        else if (!swap_a.Data.is_empty && !swap_b.Data.is_empty && !busy_a && !busy_b) {
                            var c = swap_a.Get_Swap_Info;
                            swap_a.Set_Swap_Info(swap_b.Get_Swap_Info);
                            swap_b.Set_Swap_Info(c);

                            _ = Tk_Swap_Pawn(swap_a, swap_b);
                        }

                    }
                }
            }

            is_dragging = false;
            is_drag_ready = false;
        }
        void OnDragCancel(Drag_Cancel _) {
            main_component.dragged.transform.position = out_of_screen;

            is_dragging = false;
            is_drag_ready = false;
        }



        public void On_Goblin_vs_Human(Goblin_Attack_Human args) {
            var slot_goblin = slot_look_up[(Slot_Type.A, args.goblin_slot_id)];
            var slot_human = slot_look_up[(Slot_Type.B, args.human_slot_id)];

            if (slot_goblin.Data.is_empty || slot_human.Data.is_empty)
                return;

            Tk_Goblin_attack_Human(slot_human, slot_goblin).Forget();
        }

        public void On_Human_vs_Goblin(Human_Attack_Goblin args) {
            var slot_goblin = slot_look_up[(Slot_Type.A, args.goblin_slot_id)];
            var slot_human = slot_look_up[(Slot_Type.B, args.human_slot_id)];

            if (slot_goblin.Data.is_empty || slot_human.Data.is_empty || slot_goblin.Data.goblin == null || slot_human.Data.human == null)
                return;

            Tk_Human_attack_Goblin(slot_human, slot_goblin).Forget();
        }

        async UniTaskVoid Tk_Swap_Pawn(Battlefield_Slot_Monobe slot_a, Battlefield_Slot_Monobe slot_b) {
            slot_a.Lock_Collider = true;
            slot_b.Lock_Collider = true;

            Battle_Sys.Set_Slot_Pawn_Busy(slot_a.Data, true);
            Battle_Sys.Set_Slot_Pawn_Busy(slot_b.Data, true);

            // state
            Battle_Sys.Swap_Slot_Data(slot_a.Data, slot_b.Data);

            var pawn_a = slot_a.Get_Pawn_Object;
            var pawn_b = slot_b.Get_Pawn_Object;

            _ = pawn_a.transform.DOMove(slot_b.transform.position, movetime);
            await pawn_b.transform.DOMove(slot_a.transform.position, movetime);

            slot_a.Set_Pawn_Object(pawn_b);
            slot_b.Set_Pawn_Object(pawn_a);

            Battle_Sys.Set_Slot_Pawn_Busy(slot_a.Data, false);
            Battle_Sys.Set_Slot_Pawn_Busy(slot_b.Data, false);

            slot_a.Lock_Collider = false;
            slot_b.Lock_Collider = false;
        }

        async UniTaskVoid Tk_Goblin_Goto_Bed(Battlefield_Slot_Monobe slot_a, Battlefield_Slot_Monobe slot_b) {
            slot_a.Lock_Collider = true;
            slot_b.Lock_Collider = true;

            Battle_Sys.Set_Slot_Pawn_Busy(slot_b.Data, true);

            // state
            Battle_Sys.Swap_Slot_Data(slot_a.Data, slot_b.Data);

            var pawn_a = slot_a.Get_Pawn_Object;
            var pawn_b = slot_b.Get_Pawn_Object;

            //_ = pawn_a.transform.DOMove(pawn_b.transform.position, 1);
            await pawn_b.transform.DOMove(slot_a.transform.position, movetime);

            slot_a.Set_Pawn_Object(pawn_b);
            slot_b.Set_Pawn_Object(pawn_a);

            // b-->a  a變b 寫得不清楚 容易搞錯
            Battle_Sys.Set_Slot_Pawn_Busy(slot_a.Data, false);

            slot_a.Lock_Collider = false;
            slot_b.Lock_Collider = false;
        }

        async UniTaskVoid Tk_Move_Pawn(Battlefield_Slot_Monobe slot_a, Battlefield_Slot_Monobe slot_b) {
            slot_a.Lock_Collider = true;
            slot_b.Lock_Collider = true;

            Battle_Sys.Set_Slot_Pawn_Busy(slot_b.Data, true);

            // state
            Battle_Sys.Swap_Slot_Data(slot_a.Data, slot_b.Data);

            var pawn_a = slot_a.Get_Pawn_Object;
            var pawn_b = slot_b.Get_Pawn_Object;

            //_ = pawn_a.transform.DOMove(pawn_b.transform.position, 1);
            await pawn_b.transform.DOMove(slot_a.transform.position, movetime);

            slot_a.Set_Pawn_Object(pawn_b);
            slot_b.Set_Pawn_Object(pawn_a);

            // b-->a  a變b 寫得不清楚 容易搞錯
            Battle_Sys.Set_Slot_Pawn_Busy(slot_a.Data, false);

            slot_a.Lock_Collider = false;
            slot_b.Lock_Collider = false;
        }


        async UniTaskVoid Tk_Goblin_attack_Human(Battlefield_Slot_Monobe slot_human, Battlefield_Slot_Monobe slot_goblin) {

            Human_Pawn target_human_pawn = slot_human.Data.human;
            target_human_pawn.combat.melee_queue.Add(slot_goblin.Data.id);

            slot_human.Lock_Collider = slot_human.Data.human.combat.melee_queue.Count >= 3;
            slot_goblin.Lock_Collider = true;

            var distance = math.distance(math.float2(slot_human.Data.id), math.float2(slot_goblin.Data.id));

            //Battle_Sys.Set_Slot_Pawn_Busy(slot_human.Data, true);
            Battle_Sys.Set_Slot_Pawn_Busy(slot_goblin.Data, true);

            var pawn_object = slot_goblin.Get_Pawn_Object;

            if (slot_human != null) {
                var cancel = await pawn_object.transform.DOMove(slot_human.transform.position + new Vector3(-2, 0, 0), movetime * distance).SetEase(Ease.InSine).To_Kill_Cancel_Surpress();
                if (cancel)
                    return;

                _ = slot_human.Get_Pawn_Object.transform.DOKill();
                await slot_human.Get_Pawn_Object.transform.DOLocalRotate(new Vector3(0, 0, -10), 0.1f);
                await slot_human.Get_Pawn_Object.transform.DOLocalRotate(new Vector3(0, 0, 0), 0.1f);
           
                // state
                Battle_Sys.Goblin_Attack_Human(battle_scope, target_human_pawn, slot_human.slot_id, slot_goblin.Data.goblin, slot_goblin.slot_id);

                cancel = await pawn_object.transform.DOMove(slot_goblin.transform.position, 0.4f).To_Kill_Cancel_Surpress();
                if (cancel)
                    return;
                
                cancel = await UniTask.Delay(200, cancellationToken: Game_Control.SceneLifetimeCancelToken).SuppressCancellationThrow();
                if (cancel)
                    return;

                target_human_pawn.combat.melee_queue.Remove(slot_goblin.Data.id);
                slot_human.Lock_Collider = slot_human.Data.human?.combat.melee_queue.Count < 3;

            }
            //Battle_Sys.Set_Slot_Pawn_Busy(slot_human.Data, false);
            Battle_Sys.Set_Slot_Pawn_Busy(slot_goblin.Data, false);

            slot_goblin.Lock_Collider = false;
        }

        async UniTaskVoid Tk_Human_attack_Goblin(Battlefield_Slot_Monobe slot_human, Battlefield_Slot_Monobe slot_goblin) {

            Goblin_Pawn target_goblin_pawn = slot_goblin.Data.goblin;
            target_goblin_pawn.combat.melee_queue.Add(slot_human.Data.id);

            slot_human.Lock_Collider = true;
            slot_goblin.Lock_Collider = slot_goblin.Data.goblin.combat.melee_queue.Count >= 3;

            var distance = math.distance(math.float2(slot_human.Data.id), math.float2(slot_goblin.Data.id));
            
            Battle_Sys.Set_Slot_Pawn_Busy(slot_human.Data, true);
            //Battle_Sys.Set_Slot_Pawn_Busy(slot_goblin.Data, true);

            var pawn_object = slot_human.Get_Pawn_Object;

            if (slot_goblin != null) {
                var cancel = await pawn_object.transform.DOMove(slot_goblin.transform.position + new Vector3(2, 0, 0), movetime * distance).SetEase(Ease.InSine).To_Kill_Cancel_Surpress();
                if (cancel)    
                    return;

                _ = slot_goblin.Get_Pawn_Object.transform.DOKill();
                await slot_goblin.Get_Pawn_Object.transform.DOLocalRotate(new Vector3(0, 0, 10), 0.1f);
                await slot_goblin.Get_Pawn_Object.transform.DOLocalRotate(new Vector3(0, 0, 0), 0.1f);

                // state
                Battle_Sys.Human_Attack_Goblin(battle_scope, slot_human.Data.human, slot_human.slot_id, target_goblin_pawn, slot_goblin.slot_id);

                cancel = await pawn_object.transform.DOMove(slot_human.transform.position, 0.4f).To_Kill_Cancel_Surpress();
                if (cancel)
                    return;
                
                cancel = await UniTask.Delay(200, cancellationToken: Game_Control.SceneLifetimeCancelToken).SuppressCancellationThrow();
                if (cancel)
                    return;

                target_goblin_pawn.combat.melee_queue.Remove(slot_human.Data.id);
                slot_goblin.Lock_Collider = slot_goblin.Data.goblin?.combat.melee_queue.Count < 3;

            }
            Battle_Sys.Set_Slot_Pawn_Busy(slot_human.Data, false);
            //Battle_Sys.Set_Slot_Pawn_Busy(slot_goblin.Data, false);

            slot_human.Lock_Collider = false;
        }

        public Vector3 Get_Slot_Position(Slot_Type type, int2 id) {
            var slot = slot_look_up[(type, id)];
            if (slot != null) {
                return slot.transform.position;
            }
            return out_of_screen;
        }

        public (Slot_Type type, int2 id, Vector3 pos) Get_Slot_Info(GameObject unknown_slot) {
            var slot = unknown_slot.GetComponent<Battlefield_Slot_Monobe>();
            if (slot != null)
                return (slot.slot_type, slot.slot_id, slot.transform.position);

            return (Slot_Type.U, -1, Vector3.zero);
        }

        public (Slot_Type, int2, Vector3) Scan_Target(Vector3 center, int layer) {
            var start = center;
            var num2 = Physics2D.OverlapCircleNonAlloc(start, 40, overlapResults, layer);

            System.Array.Sort(overlapResults, 0, num2, new Point_Distance_Comparer(start));

            if (num2 == 0)
                return (Slot_Type.U, new(-100, -100), Vector3.zero);

            for (int i = 0; i < num2; i++) {
                var minHit = overlapResults[i];
                var objRef = minHit.gameObject; // 用於判定目標有沒有被其他原因刪除

                if (objRef.TryGetComponent<Battlefield_Slot_Monobe>(out var slot_control)) {

                    Assert.IsNotNull(slot_control.Data);

                    if (slot_control.Data.is_empty)
                        continue;

                    return (slot_control.slot_type, slot_control.slot_id, slot_control.transform.position);
                }
            }
            return (Slot_Type.U, new (-100, -100), Vector3.zero);
        }

        public void On_Pawn_Die(Slot_Type type, int2 id) {
            var discard = slot_look_up[(type, id)];
            if (discard != null) {
                SpawnPrefabSystem.SafeReturn(discard.Get_Pawn_Object);
            }
        }

        Vector3Int tiles_base_a = new Vector3Int(-3, -4, 0);
        Vector3Int tiles_base_b = new Vector3Int(3, -4, 0);
        Vector3Int tiles_base_c = new Vector3Int(-6, -4, 0);

        Battlefield_Slot_Monobe[][] tiles_info_a;
        Battlefield_Slot_Monobe[][] tiles_info_b;
        Battlefield_Slot_Monobe[][] tiles_info_c;

        BoundsInt tiles_bounds_a = new BoundsInt(-3, -4, 0, 5, 10, 1);
        BoundsInt tiles_bounds_b = new BoundsInt(3, -4, 0, 3, 10, 1);
        BoundsInt tiles_bounds_c = new BoundsInt(-6, -4, 0, 3, 10, 1);

        void Reset_Tile_State() {
            for (int i = 0; i < tiles_info_a.Length; i++) {
                var arr = tiles_info_a[i];
                for (int j = 0; j < arr.Length; j++) {
                    var slot = arr[j];
                    slot?.Reset_Color();
                }
            }
        }


        public (int, int, int) MakeMap(System.Func<Slot_Type, int2, Slot_Data> add_slot_data) {
            Tilemap tilemap = main_component.tilemap;

            tiles_info_a = new Battlefield_Slot_Monobe[5][];
            for (int i = 0; i < 5; i++) {
                tiles_info_a[i] = new Battlefield_Slot_Monobe[10];
            }
            tiles_info_b = new Battlefield_Slot_Monobe[3][];
            for (int i = 0; i < 3; i++) {
                tiles_info_b[i] = new Battlefield_Slot_Monobe[10];
            }
            tiles_info_c = new Battlefield_Slot_Monobe[3][];
            for (int i = 0; i < 3; i++) {
                tiles_info_c[i] = new Battlefield_Slot_Monobe[10];
            }

            GameObject slot_clone = main_component.slot_clone;
            // scan
            (BoundsInt bound, int count, Slot_Type type, Color color,
             int layer, Battlefield_Slot_Monobe[][] to_tiles, Vector3Int tile_base)[] loop_setting = {
            (tiles_bounds_a, 0, Slot_Type.A, new Color32(0, 0, 255, 100),
                LayerMask.NameToLayer("Takeable_A"), tiles_info_a, tiles_base_a),
            (tiles_bounds_b, 0, Slot_Type.B, new Color32(255, 0, 255, 100),
                LayerMask.NameToLayer("Takeable_B"), tiles_info_b, tiles_base_b),
            (tiles_bounds_c, 0, Slot_Type.C, new Color32(0, 255, 0, 100),
                LayerMask.NameToLayer("Takeable_C"), tiles_info_c, tiles_base_c),
        };

            for (int s = 0; s < loop_setting.Length; s++) {
                ref var setting = ref loop_setting[s];
                for (int i = setting.bound.xMax - 1; i >= setting.bound.xMin; i--) {
                    for (int j = setting.bound.yMin; j < setting.bound.yMax; j++) {
                        var tl = tilemap.GetTile(new Vector3Int(i, j, 0));
                        if (tl != null) {

                            var new_slot = GameObject.Instantiate(slot_clone);
                            new_slot.GetComponent<SpriteRenderer>().color = setting.color;
                            new_slot.transform.position = tilemap.GetCellCenterWorld(new Vector3Int(i, j, 0));
                            new_slot.layer = setting.layer;
                            new_slot.name = $"slot {setting.type}({i}, {j})";

                            var slot_monobe = new_slot.GetComponent<Battlefield_Slot_Monobe>();
                            slot_monobe.slot_type = setting.type;
                            slot_monobe.slot_id = new (i, j);
                            slot_monobe.Reset_Original();
                            slot_monobe.Set_Data(add_slot_data?.Invoke(setting.type, new (i, j)));

                            slot_look_up.Add((slot_monobe.slot_type, slot_monobe.slot_id), slot_monobe);

                            setting.to_tiles[i - setting.tile_base.x][j - setting.tile_base.y] = slot_monobe;

                            setting.count++;
                        }
                    }
                }
            }

            return (loop_setting[0].count, loop_setting[1].count, loop_setting[2].count);
        }
    }
}
