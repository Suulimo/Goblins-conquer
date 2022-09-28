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
            PostCast,
        }

        public enum Item_Test {
            None,
            Roast_Pork,
            Birth_Drug,
            Rage_Drug,
        }

        Cursor_Mode cursor_mode = Cursor_Mode.Drag;
        Item_Test item_holding = Item_Test.None;

        public Cursor_Mode current_cursor_mode => cursor_mode;
        public Item_Test current_item_holding => item_holding;

        Battlefield_Main_Monobe main_component;

        bool is_dragging;
        bool is_drag_ready;
        bool is_picking_up;

        Slot_Type from_slot_type;
        int2 from_slot_id = new (-100, -100);
        Slot_Type selected_slot_type;
        int2 selected_slot_id = new(-100, -100);

        RaycastHit2D[] hit_2Ds = new RaycastHit2D[20];
        int pawn_layer_mask = LayerMask.GetMask("Pawn");

        Pawn_Monobe target_pawn_monobe;
        Pawn_Monobe press_pawn_monobe;
        Pawn_Monobe select_pawn_monobe;

        Battlefield_Slot_Monobe target_pawn_slot;
        Battlefield_Slot_Monobe press_pawn_slot;
        Battlefield_Slot_Monobe select_pawn_slot;

        Vector3 out_of_screen = new Vector3(-10000, 0, 0);

        Dictionary<(Slot_Type, int2), Battlefield_Slot_Monobe> slot_look_up;
        //Dictionary<Slot_Type, Battlefield_Slot_Control[]> slot_type_to_group;

        Collider2D[] overlapResults = new Collider2D[50];
        const float movetime = .15f;

        Camera camera_main = null;

        static Vector3Int
            LEFT = new Vector3Int(-1, 0, 0),
            RIGHT = new Vector3Int(1, 0, 0),
            DOWN = new Vector3Int(0, -1, 0),
            DOWNLEFT = new Vector3Int(-1, -1, 0),
            DOWNRIGHT = new Vector3Int(1, -1, 0),
            UP = new Vector3Int(0, 1, 0),
            UPLEFT = new Vector3Int(-1, 1, 0),
            UPRIGHT = new Vector3Int(1, 1, 0);

        static Vector3Int[] directions_when_y_is_even =
              { LEFT, RIGHT, DOWN, DOWNLEFT, UP, UPLEFT };
        static Vector3Int[] directions_when_y_is_odd =
              { LEFT, RIGHT, DOWN, DOWNRIGHT, UP, UPRIGHT };

        public Vector3Int[] Neighbors(Vector3Int node) {
            Vector3Int[] directions = (node.y % 2) == 0 ?
                 directions_when_y_is_even :
                 directions_when_y_is_odd;

            return directions;
        }

        Vector3Int[] test_cast_shape_y_is_even = { new(0, 0, 0), /*new(-1, 0, 0),*/ new(0, 1, 0), new(1, 0, 0), new(0, -1, 0) };
        Vector3Int[] test_cast_shape_y_is_odd = { new(0, 0, 0), /*new(-1, 0, 0),*/ new(1, 1, 0), new(1, 0, 0), new(1, -1, 0) };

        public Vector3Int[] TestCast(Vector3Int node) {
            Vector3Int[] directions = (node.y % 2) == 0 ?
                 test_cast_shape_y_is_even :
                 test_cast_shape_y_is_odd;

            return directions;
        }

        public Battlefield_Slot_Monobe Get_Slot_Monobe(Slot_Type type, int2 id) {
            return slot_look_up[(type, id)];
        }


        public Battlefield_Use(Battlefield_Main_Monobe battlefield_main_component) {
            main_component = battlefield_main_component;

            MessageBroker.Default.Receive<Drag_To>().Subscribe(OnDragTo).AddTo(main_component);
            MessageBroker.Default.Receive<Drag_To_Cancel>().Subscribe(OnDragToCancel).AddTo(main_component);
            MessageBroker.Default.Receive<Drag_Begin>().Subscribe(On_Drag_Begin).AddTo(main_component);
            MessageBroker.Default.Receive<Drag_End>().Subscribe(On_Drag_End).AddTo(main_component);
            MessageBroker.Default.Receive<Drag_Cancel>().Subscribe(OnDragCancel).AddTo(main_component);

            MessageBroker.Default.Receive<Selection_Done>().Subscribe(On_Selection_Done).AddTo(main_component);

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
            camera_main ??= Camera.main;

            _ = cursor_mode switch {
                Cursor_Mode.Drag => My_Update_Drag(dt),
                Cursor_Mode.Cast => My_Update_Cast(dt),
                Cursor_Mode.PostCast => My_Update_PostCast(dt),
                Cursor_Mode.Select => My_Update_Select(dt),
                _ => -1,
            };
        }

        int My_Update_Drag(float dt) {
            if (is_dragging) {
                var cursor = camera_main.ScreenToWorldPoint(Input.mousePosition);
                cursor.z = 0;
                main_component.dragged.transform.position = cursor + main_component.drag_hold_offset;
            }


            Ray cursor_ray = camera_main.ScreenPointToRay(Input.mousePosition);
            var num = Physics2D.RaycastNonAlloc(cursor_ray.origin, cursor_ray.direction, hit_2Ds, 500, pawn_layer_mask);
            Pawn_Monobe hit_pawn = null;
            for (int i = 0; i < num && hit_2Ds[i].collider.gameObject.TryGetComponent<Pawn_Monobe>(out var idle_pawn); i++) {
                if (idle_pawn.Get_On_Slot?.Data.is_busy_spawning_or_dying == true)
                    continue;
                if (idle_pawn.Get_On_Slot?.Data.goblin?.combat.is_busy == true)
                    continue;

                hit_pawn = idle_pawn;
                break;
            }
            var tilemap = main_component.tilemap;
            var wPos = camera_main.ScreenToWorldPoint(Input.mousePosition);
            var mouse_in_cell = tilemap.WorldToCell(wPos);
            var slot = Get_Slot_At_Position(mouse_in_cell);

            if (hit_pawn != null) {
                if (hit_pawn != select_pawn_monobe) {
                    target_pawn_monobe?.Reset_Color();
                    target_pawn_monobe = hit_pawn;
                    target_pawn_monobe.Set_Mark_Color();
                    target_pawn_slot?.Reset_Color();
                    target_pawn_slot = target_pawn_monobe.Get_On_Slot;
                    target_pawn_slot?.Set_Mark_Color();
                }
            }
            else if (slot != null) {
                if ((mouse_in_cell.x, mouse_in_cell.y) != (selected_slot_id.x, selected_slot_id.y)) {
                    target_pawn_slot?.Reset_Color();
                    target_pawn_slot = slot;
                    target_pawn_slot?.Set_Mark_Color();
                    target_pawn_monobe?.Reset_Color();
                    target_pawn_monobe = target_pawn_slot?.Get_Pawn_Monobe;
                    target_pawn_monobe?.Set_Mark_Color();
                }
            }
            else {
                target_pawn_monobe?.Reset_Color();
                target_pawn_monobe = null;

                target_pawn_slot?.Reset_Color();
                target_pawn_slot = null;
            }

            if (target_pawn_slot != null && target_pawn_slot.slot_type == Slot_Type.B)
                return 0;
            if (target_pawn_monobe != null && target_pawn_monobe.goblin_pawn == null)
                return 0;

            if (Input.GetMouseButtonDown(0)) {
                if (hit_pawn != null) {
                    press_pawn_monobe = hit_pawn;
                    press_pawn_slot = press_pawn_monobe?.Get_On_Slot;
                    On_Drag_Begin(new Drag_Begin { slot_id = hit_pawn.Get_On_Slot.slot_id, slot_type = hit_pawn.Get_On_Slot.slot_type });
                }
                else if (slot != null) {
                    if ((mouse_in_cell.x, mouse_in_cell.y) != (selected_slot_id.x, selected_slot_id.y)) {
                        On_Drag_Begin(new Drag_Begin { slot_id = slot.slot_id, slot_type = slot.slot_type });
                        press_pawn_slot = slot;
                        press_pawn_monobe = press_pawn_slot?.Get_Pawn_Monobe;
                    }
                }
            }
            else if (Input.GetMouseButtonUp(0)) {
                if (hit_pawn != null) {
                    if (press_pawn_monobe == hit_pawn) {
                        select_pawn_monobe = press_pawn_monobe;
                        select_pawn_slot = select_pawn_monobe?.Get_On_Slot;
                        press_pawn_monobe = null;
                        press_pawn_slot = null;
                        target_pawn_monobe?.Reset_Color();
                        target_pawn_monobe = null;
                        target_pawn_slot?.Reset_Color();
                        target_pawn_slot = null;

                        On_Selection_Done(new Selection_Done { slot_id = hit_pawn.Get_On_Slot.slot_id, slot_type = hit_pawn.Get_On_Slot.slot_type });
                    }
                    else {
                        is_drag_ready = true;
                        On_Drag_End(new Drag_End { slot_id = hit_pawn.Get_On_Slot.slot_id, slot_type = hit_pawn.Get_On_Slot.slot_type });
                        press_pawn_monobe = null;
                        press_pawn_slot = null;
                        target_pawn_monobe?.Reset_Color();
                        target_pawn_monobe = null;
                    }
                }
                else if (slot != null) {
                    if (press_pawn_slot == slot) {
                        select_pawn_slot = press_pawn_slot;
                        select_pawn_monobe = select_pawn_slot?.Get_Pawn_Monobe;
                        press_pawn_monobe = null;
                        press_pawn_slot = null;
                        target_pawn_monobe?.Reset_Color();
                        target_pawn_monobe = null;
                        target_pawn_slot?.Reset_Color();
                        target_pawn_slot = null;

                        On_Selection_Done(new Selection_Done { slot_id = slot.slot_id, slot_type = slot.slot_type });
                    }
                    else {
                        is_drag_ready = true;
                        On_Drag_End(new Drag_End { slot_id = slot.slot_id, slot_type = slot.slot_type });
                        press_pawn_monobe = null;
                        press_pawn_slot = null;
                        target_pawn_monobe?.Reset_Color();
                        target_pawn_monobe = null;
                    }
                }
                else {
                    On_Drag_End(new());
                }
            }
            return 0;
        }

        int My_Update_Select(float dt) {

            if (is_picking_up == false)
                return -1;

            if (Input.GetMouseButtonUp(1)) {
                On_Selection_Cancel(new());
                return 0;
            }

            Reset_Tile_State();

            var cursor = camera_main.ScreenToWorldPoint(Input.mousePosition);
            cursor.z = 0;
            main_component.dragged.transform.position = cursor + main_component.drag_hold_offset;

            Ray cursor_ray = camera_main.ScreenPointToRay(Input.mousePosition);
            var num = Physics2D.RaycastNonAlloc(cursor_ray.origin, cursor_ray.direction, hit_2Ds, 500, pawn_layer_mask);
            Pawn_Monobe hit_pawn = null;
            for (int i = 0; i < num && hit_2Ds[i].collider.gameObject.TryGetComponent<Pawn_Monobe>(out var idle_pawn); i++) {
                if (idle_pawn.Get_On_Slot?.Data.is_busy_spawning_or_dying == true)
                    continue;
                if (idle_pawn.Get_On_Slot?.Data.goblin?.combat.is_busy == true)
                    continue;

                hit_pawn = idle_pawn;
                break;
            }
            var tilemap = main_component.tilemap;
            var wPos = camera_main.ScreenToWorldPoint(Input.mousePosition);
            var mouse_in_cell = tilemap.WorldToCell(wPos);
            var slot = Get_Slot_At_Position(mouse_in_cell);
            
            if (hit_pawn != null) {
                if (hit_pawn != select_pawn_monobe) {
                    target_pawn_monobe?.Reset_Color();
                    target_pawn_monobe = hit_pawn;
                    target_pawn_monobe.Set_Mark_Color();
                    target_pawn_slot?.Reset_Color();
                    target_pawn_slot = target_pawn_monobe.Get_On_Slot;
                    target_pawn_slot?.Set_Mark_Color();
                }
            }
            else if (slot != null) {
                if ((mouse_in_cell.x, mouse_in_cell.y) != (selected_slot_id.x, selected_slot_id.y)) {
                    target_pawn_slot?.Reset_Color();
                    target_pawn_slot = slot;
                    target_pawn_slot?.Set_Mark_Color();
                    target_pawn_monobe?.Reset_Color();
                    target_pawn_monobe = target_pawn_slot?.Get_Pawn_Monobe;
                    target_pawn_monobe?.Set_Mark_Color();
                }
            }
            else {
                target_pawn_monobe?.Reset_Color();
                target_pawn_monobe = null;

                target_pawn_slot?.Reset_Color();
                target_pawn_slot = null;
            }

            if (Input.GetMouseButtonDown(0)) {
                if (hit_pawn != null) {
                    press_pawn_monobe = hit_pawn;
                    press_pawn_slot = press_pawn_monobe?.Get_On_Slot;
                }
                else if (slot != null) {
                    if ((mouse_in_cell.x, mouse_in_cell.y) != (selected_slot_id.x, selected_slot_id.y)) {
                        press_pawn_slot = slot;
                        press_pawn_monobe = press_pawn_slot?.Get_Pawn_Monobe;
                    }
                }
            }
            if (Input.GetMouseButtonUp(0)) {
                if (hit_pawn != null) {
                    if (press_pawn_monobe == hit_pawn) {
                        press_pawn_monobe = null;
                        press_pawn_slot = null;
                        target_pawn_monobe?.Reset_Color();
                        target_pawn_monobe = null;
                        target_pawn_slot?.Reset_Color();
                        target_pawn_slot = null;

                        On_Selection_Done(new Selection_Done { slot_id = hit_pawn.Get_On_Slot.slot_id, slot_type = hit_pawn.Get_On_Slot.slot_type });
                    }
                }
                else if (slot != null) {
                    if (press_pawn_slot == slot) {
                        press_pawn_slot = null;
                        press_pawn_monobe = null;
                        target_pawn_monobe?.Reset_Color();
                        target_pawn_monobe = null;
                        target_pawn_slot?.Reset_Color();
                        target_pawn_slot = null;

                        On_Selection_Done(new Selection_Done { slot_id = slot.slot_id, slot_type = slot.slot_type });
                    }
                }
                else {
                    On_Selection_Cancel(new());
                }
            }

            return 0;
        }

        int My_Update_Cast(float dt) {

            if (Input.GetMouseButtonDown(0)) {
                Test_Item_Cast();
                return 0;
            }
            if (Input.GetMouseButtonDown(1)) {
                Change_Item_Holding(Item_Test.None);
                Change_Cursor_Mode(Cursor_Mode.PostCast);
                return 0;
            }


            if (item_holding != Item_Test.None) {
                var cursor = camera_main.ScreenToWorldPoint(Input.mousePosition);
                cursor.z = 0;
                main_component.dragged.transform.position = cursor;

                main_component.dragged.sprite = (item_holding) switch {
                    Item_Test.Roast_Pork => Data_Manager.data_manager.share_pic.item_pic["roast pork"],
                    Item_Test.Rage_Drug => Data_Manager.data_manager.share_pic.item_pic["rage drug"],
                    Item_Test.Birth_Drug => Data_Manager.data_manager.share_pic.item_pic["birth drug"],
                };
            }


            Reset_Tile_State();

            var tilemap = main_component.tilemap;
            var wPos = camera_main.ScreenToWorldPoint(Input.mousePosition);

            var mouse_in_cell = tilemap.WorldToCell(wPos);

            switch (item_holding) {
                case Item_Test.Rage_Drug:
                    var cast_rage_zone = Neighbors(mouse_in_cell);
                    for (int i = 0; i < cast_rage_zone.Length; i++) {
                        var shape_i = cast_rage_zone[i];
                        var shape_cell = mouse_in_cell + shape_i;

                        if (tiles_bounds_a.Contains(shape_cell))
                            tiles_info_a[shape_cell.x - tiles_base_a.x][shape_cell.y - tiles_base_a.y]?.Set_Mark_Color();
                    }
                    break;
                case Item_Test.Birth_Drug:
                    //var neighbors = TestCast(mouse_in_cell);
                    //for (int i = 0; i < neighbors.Length; i++) {
                    //    var shape_i = neighbors[i];
                    //    var shape_cell = mouse_in_cell + shape_i;

                    //    if (tiles_bounds_a.Contains(shape_cell))
                    //        tiles_info_a[shape_cell.x - tiles_base_a.x][shape_cell.y - tiles_base_a.y]?.Set_Mark_Color();
                    //}
                    break;
                case Item_Test.Roast_Pork:
                    var cast_pork_zone = TestCast(mouse_in_cell);
                    for (int i = 0; i < cast_pork_zone.Length; i++) {
                        var shape_i = cast_pork_zone[i];
                        var shape_cell = mouse_in_cell + shape_i;

                        if (tiles_bounds_a.Contains(shape_cell))
                            tiles_info_a[shape_cell.x - tiles_base_a.x][shape_cell.y - tiles_base_a.y]?.Set_Mark_Color();
                    }
                    break;
                default:
                    break;
            }
            return 0;
        }

        int My_Update_PostCast(float dt) {
            main_component.dragged.transform.position = out_of_screen;

            Change_Cursor_Mode(Cursor_Mode.Drag);
            return 0;
        }


        public void Change_Cursor_Mode(Cursor_Mode mode) {
            cursor_mode = mode;

            Reset_Tile_State();
        }

        public void Change_Item_Holding(Item_Test item) {
            item_holding = item;
        }

        public void Test_Item_Cast() {
            if (cursor_mode == Cursor_Mode.Cast) {

                camera_main ??= Camera.main;

                Reset_Tile_State();

                var tilemap = main_component.tilemap;
                var wPos = camera_main.ScreenToWorldPoint(Input.mousePosition);

                var mouse_in_cell = tilemap.WorldToCell(wPos);

                switch (item_holding) {
                    case Item_Test.Birth_Drug:
                        battle_scope.data.inventory_birth_drug.Value--;
                        break;
                    case Item_Test.Rage_Drug:
                        battle_scope.data.inventory_rage_drug.Value--;
                        var cast_rage_zone = Neighbors(mouse_in_cell);
                        for (int i = 0; i < cast_rage_zone.Length; i++) {
                            var shape_i = cast_rage_zone[i];
                            var shape_cell = mouse_in_cell + shape_i;

                            if (tiles_bounds_a.Contains(shape_cell)) {
                                var slot_data = tiles_info_a[shape_cell.x - tiles_base_a.x][shape_cell.y - tiles_base_a.y]?.Data;
                                if (slot_data?.goblin != null) {
                                    slot_data.goblin.combat.rage_time.Value += 15;
                                }
                            }
                        }
                        break;
                    case Item_Test.Roast_Pork:
                        battle_scope.data.inventory_roast_pork_num.Value--;
                        var cast_pork_zone = TestCast(mouse_in_cell);
                        for (int i = 0; i < cast_pork_zone.Length; i++) {
                            var shape_i = cast_pork_zone[i];
                            var shape_cell = mouse_in_cell + shape_i;

                            if (tiles_bounds_a.Contains(shape_cell)) {
                                var slot_data = tiles_info_a[shape_cell.x - tiles_base_a.x][shape_cell.y - tiles_base_a.y]?.Data;
                                if (slot_data?.goblin != null) {
                                    slot_data.goblin.combat.hp.Value = Mathf.Min(slot_data.goblin.combat.hp.Value + 50, slot_data.goblin.combat.hp_max);
                                }
                            }
                        }
                        break;
                    default:
                        break;
                }

                Change_Item_Holding(Item_Test.None);
                Change_Cursor_Mode(Cursor_Mode.PostCast);
            }
        }

        void OnDragTo(Drag_To args) {
            if (is_dragging == false)
                return;

            from_slot_type = args.slot_type;
            from_slot_id = args.slot_id;

            is_drag_ready = true;
        }

        void OnDragToCancel(Drag_To_Cancel _) {
            if (is_dragging == false)
                return;
            is_drag_ready = false;
        }

        void On_Drag_Begin(Drag_Begin args) {
            if (is_dragging)
                return;

            if (args.slot_type == Slot_Type.U)
                return;

            var slot = slot_look_up[(args.slot_type, args.slot_id)];
            if (slot == null)
                return;

            select_pawn_slot = slot;
            slot.Set_Selected_Color();

            if (slot.Get_Pawn_Object != null && slot.Get_Pawn_Object.TryGetComponent<Pawn_Monobe>(out var pawn_monobe)) {
                select_pawn_monobe = pawn_monobe;
                pawn_monobe.Set_Selected_Color();
            }

            var info = slot.Get_Swap_Info;
            //main_component.dragged.color = slot.Get_Swap_Info.color;
            main_component.dragged.sprite = info.sprite;
            main_component.dragged.flipX = info.flip_x;
            var cursor = camera_main.ScreenToWorldPoint(Input.mousePosition);
            cursor.z = 0;

            main_component.drag_hold_offset = slot.Get_Pawn_Object?.transform.position - cursor ?? Vector3.zero;

            from_slot_id = args.slot_id;
            from_slot_type = args.slot_type;

            is_dragging = true;
        }

        void On_Drag_End(Drag_End args) {

            main_component.dragged.transform.position = out_of_screen;


            if (is_dragging && is_drag_ready) {

                if (args.slot_type == Slot_Type.U) {
                    // 丟棄
                    var discard = slot_look_up[(from_slot_type, from_slot_id)];
                    if (discard != null) {
                        SpawnPrefabSystem.SafeReturn(discard.Get_Pawn_Object);
                        SpawnPrefabSystem.SafeReturn(discard.Get_Slot_Display_Object);
                        Battle_Sys.Discard_Pawn_At(Static_Game_Scope.battle_scope, from_slot_type, from_slot_id);
                    }

                }
                else {
                    Swap_or_Move(args, from_slot_type, from_slot_id);
                }
            }

            is_dragging = false;
            is_drag_ready = false;
        }

        private void Swap_or_Move(Drag_End args, Slot_Type from_type, int2 from_id) {
            // 交換或移動
            var swap_a = slot_look_up[(args.slot_type, args.slot_id)];
            var swap_b = slot_look_up[(from_type, from_id)];

            if (swap_a && swap_b && (swap_a.slot_type == swap_b.slot_type)) {

                bool busy_a = Battle_Sys.Ask_Slot_Is_Busy(swap_a.Data) || swap_a.Data.is_busy_spawning_or_dying;
                bool busy_b = Battle_Sys.Ask_Slot_Is_Busy(swap_b.Data) || swap_b.Data.is_busy_spawning_or_dying;

                if (args.slot_type == Slot_Type.C) {
                    if (!swap_a.Data.is_empty && !swap_b.Data.is_empty && !busy_a && !busy_b && swap_a.Data.bed != null && swap_b.Data.bed != null) {
                        var c = swap_a.Get_Swap_Info;
                        swap_a.Set_Swap_Info(swap_b.Get_Swap_Info);
                        swap_b.Set_Swap_Info(c);
                        _ = Tk_Swap_Pawn(swap_a, swap_b);
                    }
                    else if (swap_a.Data.is_empty && swap_b.Data.is_empty == false && swap_a.Data.bed != null && !busy_b) {
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
                    else if (swap_a.Data.is_empty && swap_b.Data.is_empty == false && !busy_b) {
                        var c = swap_a.Get_Swap_Info;
                        swap_a.Set_Swap_Info(swap_b.Get_Swap_Info);
                        swap_b.Set_Swap_Info(c);
                        _ = Tk_Move_Pawn(swap_a, swap_b);
                    }
                }
            }

            // siu息
            if (swap_a && swap_b && (args.slot_type, from_type) == (Slot_Type.C, Slot_Type.A)) {

                bool busy_a = Battle_Sys.Ask_Slot_Is_Busy(swap_a.Data) || swap_a.Data.is_busy_spawning_or_dying;
                bool busy_b = Battle_Sys.Ask_Slot_Is_Busy(swap_b.Data) || swap_b.Data.is_busy_spawning_or_dying;

                if (swap_a.Data.is_empty && swap_b.Data.is_empty == false && swap_a.Data.bed != null && !busy_b) {
                    _ = Tk_Goblin_Goto_Bed(swap_a, swap_b);
                }
                else if (!swap_a.Data.is_empty && !swap_b.Data.is_empty && !busy_a && !busy_b) {
                    var c = swap_a.Get_Swap_Info;
                    swap_a.Set_Swap_Info(swap_b.Get_Swap_Info);
                    swap_b.Set_Swap_Info(c);

                    _ = Tk_Swap_Pawn(swap_a, swap_b);
                }

            }
            else if (swap_a && swap_b && (args.slot_type, from_type) == (Slot_Type.A, Slot_Type.C)) {

                bool busy_a = Battle_Sys.Ask_Slot_Is_Busy(swap_a.Data) || swap_a.Data.is_busy_spawning_or_dying;
                bool busy_b = Battle_Sys.Ask_Slot_Is_Busy(swap_b.Data) || swap_b.Data.is_busy_spawning_or_dying;

                if (swap_a.Data.is_empty && swap_b.Data.is_empty == false && !busy_a) {
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

        void OnDragCancel(Drag_Cancel _) {
            main_component.dragged.transform.position = out_of_screen;

            is_dragging = false;
            is_drag_ready = false;
        }

        void On_Selection_Done(Selection_Done args) {

            if (cursor_mode == Cursor_Mode.Cast || cursor_mode == Cursor_Mode.PostCast)
                return;

            if (!is_picking_up) {

                if (args.slot_type == Slot_Type.U)
                    return;

                var slot = slot_look_up[(args.slot_type, args.slot_id)];
                if (slot == null || slot.Data.is_empty == true)
                    return;

                Change_Cursor_Mode(GCQ.Battlefield_Use.Cursor_Mode.Select);

                First_Seletion_Pick(args);
                is_picking_up = true;
            }
            else {
                Second_Selection_Drop(args);
            }
        }

        void First_Seletion_Pick(Selection_Done args) {
            var slot = slot_look_up[(args.slot_type, args.slot_id)];
            if (slot == null)
                return;

            select_pawn_slot = slot;
            slot.Set_Selected_Color();

            if (slot.Get_Pawn_Object.TryGetComponent<Pawn_Monobe>(out var pawn_monobe)) {
                select_pawn_monobe = pawn_monobe;
                pawn_monobe.Set_Selected_Color();
            }

            var info = slot.Get_Swap_Info;
            //main_component.dragged.color = slot.Get_Swap_Info.color;
            main_component.dragged.sprite = info.sprite;
            main_component.dragged.flipX = info.flip_x;
            main_component.dragged.color = Color.cyan;
            var cursor = camera_main.ScreenToWorldPoint(Input.mousePosition);
            cursor.z = 0;

            main_component.drag_hold_offset = slot.Get_Pawn_Object.transform.position - cursor;

            selected_slot_id = args.slot_id;
            selected_slot_type = args.slot_type;
        }

        void Second_Selection_Drop(Selection_Done args) {

            if (args.slot_type == Slot_Type.U) {
                // 丟棄
                var discard = slot_look_up[(selected_slot_type, selected_slot_id)];
                if (discard != null) {
                    SpawnPrefabSystem.SafeReturn(discard.Get_Pawn_Object);
                    SpawnPrefabSystem.SafeReturn(discard.Get_Slot_Display_Object);
                    Battle_Sys.Discard_Pawn_At(Static_Game_Scope.battle_scope, from_slot_type, from_slot_id);
                }
            }
            else if ((args.slot_type, args.slot_id.x, args.slot_id.y) != (selected_slot_type, selected_slot_id.x, selected_slot_id.y)) {
                Swap_or_Move(new Drag_End { slot_type = args.slot_type, slot_id = args.slot_id }, selected_slot_type, selected_slot_id);
            }
            else {
            }

            On_Selection_Cancel(new());
        }

        void On_Selection_Cancel(Selection_Cancel _) {
            main_component.dragged.transform.position = out_of_screen;

            is_picking_up = false;

            Reset_Tile_State();
            select_pawn_monobe?.Reset_Color();
            select_pawn_monobe = null;

            target_pawn_monobe?.Reset_Color();
            target_pawn_monobe = null;

            GCQ.Static_Game_Scope.battlefield_main_ref.Use.Change_Cursor_Mode(GCQ.Battlefield_Use.Cursor_Mode.Drag);
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

            if (pawn_a != null && pawn_a.TryGetComponent<Pawn_Monobe>(out var pawn_monobe_a)) {
                pawn_monobe_a.Set_On_Slot(slot_b);
            }
            if (pawn_b != null && pawn_b.TryGetComponent<Pawn_Monobe>(out var pawn_monobe_b)) {
                pawn_monobe_b.Set_On_Slot(slot_a);
            }



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

            if (pawn_a != null && pawn_a.TryGetComponent<Pawn_Monobe>(out var pawn_monobe_a)) {
                pawn_monobe_a.Set_On_Slot(slot_b);
            }
            if (pawn_b != null && pawn_b.TryGetComponent<Pawn_Monobe>(out var pawn_monobe_b)) {
                pawn_monobe_b.Set_On_Slot(slot_a);
            }
            
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

            if (pawn_a != null &&  pawn_a.TryGetComponent<Pawn_Monobe>(out var pawn_monobe_a)) {
                pawn_monobe_a.Set_On_Slot(slot_b);
            }
            if (pawn_b != null && pawn_b.TryGetComponent<Pawn_Monobe>(out var pawn_monobe_b)) {
                pawn_monobe_b.Set_On_Slot(slot_a);
            }
            
            // b-->a  a變b 寫得不清楚 容易搞錯
            Battle_Sys.Set_Slot_Pawn_Busy(slot_a.Data, false);

            slot_a.Lock_Collider = false;
            slot_b.Lock_Collider = false;
        }


        async UniTaskVoid Tk_Goblin_attack_Human(Battlefield_Slot_Monobe slot_human, Battlefield_Slot_Monobe slot_goblin) {

            Human_Pawn target_human_pawn = slot_human.Data.human;
            target_human_pawn.combat.melee_queue.Add(slot_goblin.Data.id);
            Goblin_Pawn acting_goblin_pawn = slot_goblin.Data.goblin;

            slot_human.Lock_Collider = slot_human.Data.human.combat.melee_queue.Count >= 3;
            slot_goblin.Lock_Collider = true;

            var distance = math.distance(math.float2(slot_human.Data.id), math.float2(slot_goblin.Data.id));

            //Battle_Sys.Set_Slot_Pawn_Busy(slot_human.Data, true);
            Battle_Sys.Set_Slot_Pawn_Busy(slot_goblin.Data, true);

            var pawn_object = slot_goblin.Get_Pawn_Object;

            if (slot_human != null) {
                var cancel = await pawn_object.transform.DOMove(slot_human.transform.position + new Vector3(-2, 0, 0), movetime * distance).To_Kill_Cancel_Surpress();
                if (cancel)
                    return;

                if (slot_human.Data.is_busy_spawning_or_dying == false) {
                    //_ = slot_human.Get_Pawn_Monobe.Set_On_Hit_Color();
                    _ = slot_human.Get_Pawn_Object?.transform.DOKill();
                    await slot_human.Get_Pawn_Object?.transform.DOLocalRotate(new Vector3(0, 0, -10), 0.1f);
                    await slot_human.Get_Pawn_Object?.transform.DOLocalRotate(new Vector3(0, 0, 0), 0.1f);
                }
                // state
                Battle_Sys.Goblin_Attack_Human(battle_scope, target_human_pawn, slot_human.slot_id, acting_goblin_pawn, slot_goblin.slot_id);

                cancel = await pawn_object.transform.DOMove(slot_goblin.transform.position, 0.4f).To_Kill_Cancel_Surpress();
                if (cancel)
                    return;
                
                cancel = await UniTask.Delay(200, cancellationToken: Game_Control.SceneLifetimeCancelToken).SuppressCancellationThrow();
                if (cancel)
                    return;

                target_human_pawn.combat.melee_queue.Remove(slot_goblin.Data.id);
                slot_human.Lock_Collider = slot_human.Data.human?.combat.melee_queue.Count >= 3;

            }
            //Battle_Sys.Set_Slot_Pawn_Busy(slot_human.Data, false);
            Battle_Sys.Set_Slot_Pawn_Busy(slot_goblin.Data, false);

            slot_goblin.Lock_Collider = false;
        }

        async UniTaskVoid Tk_Human_attack_Goblin(Battlefield_Slot_Monobe slot_human, Battlefield_Slot_Monobe slot_goblin) {

            Goblin_Pawn target_goblin_pawn = slot_goblin.Data.goblin;
            target_goblin_pawn.combat.melee_queue.Add(slot_human.Data.id);
            Human_Pawn acting_human_pawn = slot_human.Data.human;

            slot_human.Lock_Collider = true;
            slot_goblin.Lock_Collider = slot_goblin.Data.goblin.combat.melee_queue.Count >= 3;

            var distance = math.distance(math.float2(slot_human.Data.id), math.float2(slot_goblin.Data.id));
            
            Battle_Sys.Set_Slot_Pawn_Busy(slot_human.Data, true);
            //Battle_Sys.Set_Slot_Pawn_Busy(slot_goblin.Data, true);

            var pawn_object = slot_human.Get_Pawn_Object;

            if (slot_goblin != null) {
                var cancel = await pawn_object.transform.DOMove(slot_goblin.transform.position + new Vector3(2, 0, 0), movetime * distance).To_Kill_Cancel_Surpress();
                if (cancel)    
                    return;

                if (slot_goblin.Data.is_busy_spawning_or_dying == false) {
                    //_ = slot_goblin.Get_Pawn_Monobe.Set_On_Hit_Color();
                    _ = slot_goblin.Get_Pawn_Object?.transform.DOKill();
                    await slot_goblin.Get_Pawn_Object?.transform.DOLocalRotate(new Vector3(0, 0, 10), 0.1f);
                    await slot_goblin.Get_Pawn_Object?.transform.DOLocalRotate(new Vector3(0, 0, 0), 0.1f);
                }
                // state
                Battle_Sys.Human_Attack_Goblin(battle_scope, acting_human_pawn, slot_human.slot_id, target_goblin_pawn, slot_goblin.slot_id);

                cancel = await pawn_object.transform.DOMove(slot_human.transform.position, 0.4f).To_Kill_Cancel_Surpress();
                if (cancel)
                    return;
                
                cancel = await UniTask.Delay(200, cancellationToken: Game_Control.SceneLifetimeCancelToken).SuppressCancellationThrow();
                if (cancel)
                    return;

                target_goblin_pawn.combat.melee_queue.Remove(slot_human.Data.id);
                slot_goblin.Lock_Collider = slot_goblin.Data.goblin?.combat.melee_queue.Count >= 3;

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

                    if (slot_control.Data.is_empty || slot_control.Data.is_busy_spawning_or_dying)
                        continue;

                    return (slot_control.slot_type, slot_control.slot_id, slot_control.transform.position);
                }
            }
            return (Slot_Type.U, new (-100, -100), Vector3.zero);
        }

        public void On_Pawn_Die(Slot_Type type, int2 id) {
            var discard = slot_look_up[(type, id)];
            
            if (discard != null) {
                var jump_dir = (type) switch {
                    Slot_Type.A => new Vector3(-60, 0, 0),
                    Slot_Type.B => new Vector3(30, 0, 0),
                    _ => new Vector3(0, 100, 0),
                };

                var rotate_dir = (type) switch {
                    Slot_Type.A => new Vector3(0, 0, 720),
                    Slot_Type.B => new Vector3(0, 0, -720),
                    _ => Vector3.zero,
                };


                discard.Data.is_busy_spawning_or_dying = true;
                discard.Get_Pawn_Object.transform.DOLocalRotate(rotate_dir, 1.0f, RotateMode.FastBeyond360).SetEase(Ease.Linear);
                discard.Get_Pawn_Object.transform.DOLocalJump(jump_dir, 5, 1, 1.0f).SetEase(Ease.Linear).OnComplete(() => {
                    discard.Data.is_busy_spawning_or_dying = false;
                    SpawnPrefabSystem.SafeReturn(discard.Get_Pawn_Object);
                    discard.Set_Pawn_Object(null);
                });
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

        Vector3Int trashcan_slot_xy = new Vector3Int(-6, -5, 0);
        Battlefield_Slot_Monobe trashcan_slot;

        Battlefield_Slot_Monobe Get_Slot_At_Position(Vector3Int pos) {
            if (tiles_bounds_a.Contains(pos))
                return tiles_info_a[pos.x - tiles_base_a.x][pos.y - tiles_base_a.y];
            if (tiles_bounds_b.Contains(pos))
                return tiles_info_b[pos.x - tiles_base_b.x][pos.y - tiles_base_b.y];
            if (tiles_bounds_c.Contains(pos))
                return tiles_info_c[pos.x - tiles_base_c.x][pos.y - tiles_base_c.y];
            if (pos.x == trashcan_slot_xy.x && pos.y == trashcan_slot_xy.y)
                return trashcan_slot;
            return null;
        }

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

            var new_trash_obj = GameObject.Instantiate(slot_clone);
            new_trash_obj.GetComponent<SpriteRenderer>().color = Color.white;
            new_trash_obj.transform.position = tilemap.GetCellCenterWorld(new Vector3Int(-4, -6, 0));
            new_trash_obj.layer = LayerMask.NameToLayer("Default");
            new_trash_obj.name = $"slot (trashcan)";

            trashcan_slot = new_trash_obj.GetComponent<Battlefield_Slot_Monobe>();
            trashcan_slot.slot_type = Slot_Type.U;
            trashcan_slot.slot_id = new(-4, -6);
            trashcan_slot.Reset_Original();

            return (loop_setting[0].count, loop_setting[1].count, loop_setting[2].count);
        }
    }
}
