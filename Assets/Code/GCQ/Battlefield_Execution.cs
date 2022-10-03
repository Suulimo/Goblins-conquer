using UnityEngine;
using DG.Tweening;
using Cysharp.Threading.Tasks;
using Unity.Mathematics;

namespace GCQ
{
    public static class Battlefield_Execution
    {
        const float movetime = .15f;

        public static void On_Spawn_Goblin(Goblin_Spawned args) {
            var to_slot = args.battle_scope_ref.battlefield_main_ref.Use.Get_Slot_Monobe(Slot_Type.A, args.slot_id);
            if (to_slot != null) {
                var p = Rent_System.Rent_Pawn_Monobe("Pawn", to_slot.transform, false);

                if (p != null) {
                    p.sprite_renderer.sprite = Data_Manager.data_manager.share_pic.goblin_pic[args.goblin_data.other.sprite];
                    p.sprite_renderer.flipX = true;
                    p.Set_Goblin_Pawn(to_slot.Data.goblin);
                    p.Set_On_Slot(to_slot);
                    to_slot.Set_Pawn_Object(p.gameObject);
                }
            }
        }

        public static void On_Spawn_Human(Human_Spawned args) {
            var to_slot = args.battle_scope_ref.battlefield_main_ref.Use.Get_Slot_Monobe(Slot_Type.B, args.slot_id);
            if (to_slot != null) {
                var p = Rent_System.Rent_Pawn_Monobe("Pawn", to_slot.transform, false);

                if (p != null) {
                    p.sprite_renderer.sprite = Data_Manager.data_manager.share_pic.human_pic[args.human_data.other.sprite];
                    p.sprite_renderer.flipX = false;
                    p.Set_Human_Pawn(to_slot.Data.human);
                    p.Set_On_Slot(to_slot);

                    to_slot.Data.is_busy_spawning_or_dying = true;

                    p.transform.DOLocalMoveX(20, 0.5f).SetInverted().OnComplete(() => {
                        to_slot.Data.is_busy_spawning_or_dying = false;
                    });
                    to_slot.Set_Pawn_Object(p.gameObject);
                }
            }
        }

        public static void On_Spawn_Bed(Bed_Spawned args) {
            var to_slot = args.battle_scope_ref.battlefield_main_ref.Use.Get_Slot_Monobe(Slot_Type.C, args.slot_id);
            if (to_slot != null) {
                var p = Rent_System.Rent_Slot_Display_Monobe("Temp Slot Display", to_slot.transform, false);

                if (p != null) {
                    if (p.TryGetComponent<SpriteRenderer>(out var render)) { 
                        render.sprite = Data_Manager.data_manager.share_pic.bed_pic[args.human_data.other.sprite_bed];
                        render.flipX = false;
                    }
                    p.Set_Bed_Pawn(to_slot.Data.bed);
                }

                to_slot.Set_Slot_Display_Object(p);
            }
        }

        public static async UniTaskVoid Tk_Human_attack_Goblin(Human_Attack_Goblin args) {
            var use = args.battle_scope_ref.battlefield_main_ref.Use;
            var slot_goblin = use.slot_look_up[(Slot_Type.A, args.goblin_slot_id)];
            var slot_human = use.slot_look_up[(Slot_Type.B, args.human_slot_id)];

            if (slot_goblin.Data.is_empty || slot_human.Data.is_empty || slot_goblin.Data.goblin == null || slot_human.Data.human == null)
                return;

            Goblin_Pawn target_goblin_pawn = slot_goblin.Data.goblin;
            target_goblin_pawn.combat.melee_queue.Add(slot_human.Data.id);
            Human_Pawn acting_human_pawn = slot_human.Data.human;

            slot_human.Lock_Collider = true;
            slot_goblin.Lock_Collider = slot_goblin.Data.goblin.combat.melee_queue.Count >= 3;

            var distance = math.distance(math.float2(slot_human.Data.id), math.float2(slot_goblin.Data.id));

            Battle_Sys.Set_Pawn_Busy(acting_human_pawn, true);
            //Battle_Sys.Set_Pawn_Busy(slot_goblin.Data, true);

            var pawn_object = slot_human.Get_Pawn_Object;

            if (slot_goblin != null && pawn_object != null) {
                if (await pawn_object.transform.DOMove(slot_goblin.transform.position + new Vector3(2, 0, 0), movetime * distance).If_Cancel()) return;

                if (slot_goblin.Data.is_busy_spawning_or_dying == false) {
                    //_ = slot_goblin.Get_Pawn_Monobe.Set_On_Hit_Color();
                    _ = slot_goblin.Get_Pawn_Object?.transform.DOKill();
                    if (await (slot_goblin.Get_Pawn_Object?.transform.DOLocalRotate(new Vector3(0, 0, 10), 0.1f).If_Cancel() ?? My_DOTween_Async_Extensions.Not_Cancel)) {
                        Debug.LogError("cancel");
                        return;
                    }
                    if (await (slot_goblin.Get_Pawn_Object?.transform.DOLocalRotate(new Vector3(0, 0, 0), 0.1f).If_Cancel() ?? My_DOTween_Async_Extensions.Not_Cancel)) {
                        Debug.LogError("cancel");
                        return;
                    }
                }
                // state
                Battle_Sys.Human_Attack_Goblin(args.battle_scope_ref, acting_human_pawn, slot_human.slot_id, target_goblin_pawn, slot_goblin.slot_id);

                if (await pawn_object.transform.DOMove(slot_human.transform.position, 0.4f).If_Cancel()) return;

                //if (await UniTask.Delay(200, cancellationToken: Game_Control.SceneLifetimeCancelToken).SuppressCancellationThrow()) return;

                target_goblin_pawn.combat.melee_queue.Remove(slot_human.Data.id);
                slot_goblin.Lock_Collider = slot_goblin.Data.goblin?.combat.melee_queue.Count >= 3;

            }
            Battle_Sys.Set_Pawn_Busy(acting_human_pawn, false);
            //Battle_Sys.Set_Pawn_Busy(slot_goblin.Data, false);

            slot_human.Lock_Collider = false;
        }

        public static async UniTaskVoid Tk_Goblin_attack_Human(Goblin_Attack_Human args) {
            var use = args.battle_scope_ref.battlefield_main_ref.Use;
            var slot_goblin = use.slot_look_up[(Slot_Type.A, args.goblin_slot_id)];
            var slot_human = use.slot_look_up[(Slot_Type.B, args.human_slot_id)];

            if (slot_goblin.Data.is_empty || slot_human.Data.is_empty)
                return;

            Human_Pawn target_human_pawn = slot_human.Data.human;
            target_human_pawn.combat.melee_queue.Add(slot_goblin.Data.id);
            Goblin_Pawn acting_goblin_pawn = slot_goblin.Data.goblin;

            slot_human.Lock_Collider = slot_human.Data.human.combat.melee_queue.Count >= 3;
            slot_goblin.Lock_Collider = true;

            var distance = math.distance(math.float2(slot_human.Data.id), math.float2(slot_goblin.Data.id));

            //Battle_Sys.Set_Pawn_Busy(slot_human.Data, true);
            Battle_Sys.Set_Pawn_Busy(acting_goblin_pawn, true);

            var pawn_object = slot_goblin.Get_Pawn_Object;

            if (slot_human != null && pawn_object != null) {
                if (pawn_object == null)
                    Debug.LogError("p gob null");

                var dest = slot_human.transform.position + (pawn_object.transform.position - slot_human.transform.position).normalized + new Vector3(-1, 0, 0);

                if (await pawn_object.transform.DOMove(dest, movetime * distance).If_Cancel()) return;

                if (slot_human.Data.is_busy_spawning_or_dying == false) {
                    //_ = slot_human.Get_Pawn_Monobe.Set_On_Hit_Color();
                    slot_human.Get_Pawn_Object?.transform.DOKill();
                    if (await (slot_human.Get_Pawn_Object?.transform.DOLocalRotate(new Vector3(0, 0, -10), 0.1f).If_Cancel() ?? My_DOTween_Async_Extensions.Not_Cancel)) {
                        Debug.LogError("cancel");
                        return;
                    }
                    if (await (slot_human.Get_Pawn_Object?.transform.DOLocalRotate(new Vector3(0, 0, 0), 0.1f).If_Cancel() ?? My_DOTween_Async_Extensions.Not_Cancel)) {
                        Debug.LogError("cancel");
                        return;
                    }
                }
                // state
                Battle_Sys.Goblin_Attack_Human(args.battle_scope_ref, target_human_pawn, slot_human.slot_id, acting_goblin_pawn, slot_goblin.slot_id);

                if (await pawn_object.transform.DOMove(slot_goblin.transform.position, 0.4f).If_Cancel()) return;

                //if (await UniTask.Delay(200, cancellationToken: Game_Control.SceneLifetimeCancelToken).SuppressCancellationThrow()) return;

                target_human_pawn.combat.melee_queue.Remove(slot_goblin.Data.id);
                slot_human.Lock_Collider = slot_human.Data.human?.combat.melee_queue.Count >= 3;

            }
            //Battle_Sys.Set_Pawn_Busy(slot_human.Data, false);
            Battle_Sys.Set_Pawn_Busy(acting_goblin_pawn, false);

            slot_goblin.Lock_Collider = false;
        }

        public static async UniTaskVoid Tk_Swap_Pawn(Battlefield_Slot_Monobe slot_a, Battlefield_Slot_Monobe slot_b) {
            slot_a.Lock_Collider = true;
            slot_b.Lock_Collider = true;

            Goblin_Pawn slot_a_goblin_pawn = slot_a.Data.goblin;
            Goblin_Pawn slot_b_goblin_pawn = slot_b.Data.goblin;
            Human_Pawn slot_a_human_pawn = slot_a.Data.human;
            Human_Pawn slot_b_human_pawn = slot_b.Data.human;

            Battle_Sys.Set_Pawn_Busy(slot_a_goblin_pawn, true);
            Battle_Sys.Set_Pawn_Busy(slot_b_goblin_pawn, true);
            Battle_Sys.Set_Pawn_Busy(slot_a_human_pawn, true);
            Battle_Sys.Set_Pawn_Busy(slot_b_human_pawn, true);

            // state
            Battle_Sys.Swap_Slot_Data(slot_a.Data, slot_b.Data);

            var pawn_a = slot_a.Get_Pawn_Object;
            var pawn_b = slot_b.Get_Pawn_Object;

            if (pawn_a != null)
                _ = pawn_a.transform.DOMove(slot_b.transform.position, movetime);
            if (pawn_b != null)
                if (await pawn_b.transform.DOMove(slot_a.transform.position, movetime).If_Cancel()) return;

            slot_a.Set_Pawn_Object(pawn_b);
            slot_b.Set_Pawn_Object(pawn_a);

            if (pawn_a != null && pawn_a.TryGetComponent<Pawn_Monobe>(out var pawn_monobe_a)) {
                pawn_monobe_a.Set_On_Slot(slot_b);
            }
            if (pawn_b != null && pawn_b.TryGetComponent<Pawn_Monobe>(out var pawn_monobe_b)) {
                pawn_monobe_b.Set_On_Slot(slot_a);
            }

            Battle_Sys.Set_Pawn_Busy(slot_a_goblin_pawn, false);
            Battle_Sys.Set_Pawn_Busy(slot_b_goblin_pawn, false);
            Battle_Sys.Set_Pawn_Busy(slot_a_human_pawn, false);
            Battle_Sys.Set_Pawn_Busy(slot_b_human_pawn, false);

            slot_a.Lock_Collider = false;
            slot_b.Lock_Collider = false;
        }

        public static async UniTaskVoid Tk_Goblin_Goto_Bed(Battlefield_Slot_Monobe slot_a, Battlefield_Slot_Monobe slot_b) {
            slot_a.Lock_Collider = true;
            slot_b.Lock_Collider = true;

            Goblin_Pawn acting_goblin_pawn = slot_b.Data.goblin;

            Battle_Sys.Set_Pawn_Busy(acting_goblin_pawn, true);

            // state
            Battle_Sys.Swap_Slot_Data(slot_a.Data, slot_b.Data);

            var pawn_a = slot_a.Get_Pawn_Object;
            var pawn_b = slot_b.Get_Pawn_Object;

            //_ = pawn_a.transform.DOMove(pawn_b.transform.position, 1);
            if (pawn_b != null)
                if (await pawn_b.transform.DOMove(slot_a.transform.position, movetime).If_Cancel()) return;

            slot_a.Set_Pawn_Object(pawn_b);
            slot_b.Set_Pawn_Object(pawn_a);

            if (pawn_a != null && pawn_a.TryGetComponent<Pawn_Monobe>(out var pawn_monobe_a)) {
                pawn_monobe_a.Set_On_Slot(slot_b);
            }
            if (pawn_b != null && pawn_b.TryGetComponent<Pawn_Monobe>(out var pawn_monobe_b)) {
                pawn_monobe_b.Set_On_Slot(slot_a);
            }

            // b-->a  a變b 寫得不清楚 容易搞錯
            Battle_Sys.Set_Pawn_Busy(acting_goblin_pawn, false);

            slot_a.Lock_Collider = false;
            slot_b.Lock_Collider = false;
        }

        public static async UniTaskVoid Tk_Move_Pawn(Battlefield_Slot_Monobe slot_a, Battlefield_Slot_Monobe slot_b) {
            slot_a.Lock_Collider = true;
            slot_b.Lock_Collider = true;

            Goblin_Pawn slot_b_goblin_pawn = slot_b.Data.goblin;
            Human_Pawn slot_b_human_pawn = slot_b.Data.human;

            Battle_Sys.Set_Pawn_Busy(slot_b_goblin_pawn, true);
            Battle_Sys.Set_Pawn_Busy(slot_b_human_pawn, true);

            // state
            Battle_Sys.Swap_Slot_Data(slot_a.Data, slot_b.Data);

            var pawn_a = slot_a.Get_Pawn_Object;
            var pawn_b = slot_b.Get_Pawn_Object;

            //_ = pawn_a.transform.DOMove(pawn_b.transform.position, 1);
            if (pawn_b != null)
                if (await pawn_b.transform.DOMove(slot_a.transform.position, movetime).If_Cancel()) {
                    Debug.LogError("cancel");
                    return;
                }

            slot_a.Set_Pawn_Object(pawn_b);
            slot_b.Set_Pawn_Object(pawn_a);

            if (pawn_a != null && pawn_a.TryGetComponent<Pawn_Monobe>(out var pawn_monobe_a)) {
                pawn_monobe_a.Set_On_Slot(slot_b);
            }
            if (pawn_b != null && pawn_b.TryGetComponent<Pawn_Monobe>(out var pawn_monobe_b)) {
                pawn_monobe_b.Set_On_Slot(slot_a);
            }

            // b-->a  a變b 寫得不清楚 容易搞錯
            Battle_Sys.Set_Pawn_Busy(slot_b_goblin_pawn, false);
            Battle_Sys.Set_Pawn_Busy(slot_b_human_pawn, false);

            slot_a.Lock_Collider = false;
            slot_b.Lock_Collider = false;
        }

        public static void On_Pawn_Die(Pawn_Die_Msg msg) {
            var use = msg.battle_scope_ref.battlefield_main_ref.Use;
            var discard = use.slot_look_up[(msg.type, msg.id)];

            if (discard != null) {
                var jump_dir = (msg.type) switch {
                    Slot_Type.A => new Vector3(-60, 0, 0),
                    Slot_Type.B => new Vector3(30, 0, 0),
                    _ => new Vector3(0, 100, 0),
                };

                var rotate_dir = (msg.type) switch {
                    Slot_Type.A => new Vector3(0, 0, 720),
                    Slot_Type.B => new Vector3(0, 0, -720),
                    _ => Vector3.zero,
                };

                var pawn = discard.Get_Pawn_Monobe;
                var display = discard.Get_Slot_Display_Object;

                discard.Set_Pawn_Object(null);
                discard.Set_Slot_Display_Object(null);

                discard.Data.is_busy_spawning_or_dying = true;

                pawn.transform.DOLocalRotate(rotate_dir, 1.0f, RotateMode.FastBeyond360).SetEase(Ease.Linear);
                pawn.transform.DOLocalJump(jump_dir, 5, 1, 1.0f).SetEase(Ease.Linear).OnComplete(() => {
                    discard.Data.is_busy_spawning_or_dying = false;
                    display?.Return_Self();
                    pawn?.Return_Self();
                });
            }
        }

    }
}