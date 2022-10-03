using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GCQ
{
    public static class Babamama_Sys
    {
        [ClearOnReload] public static Dictionary<(string, string), Goblin_Spec> babamama_table;
        [ClearOnReload] public static Dictionary<string, Goblin_Spec> babaonly_table;
        [ClearOnReload] public static Dictionary<string, Goblin_Spec> mamaonly_table;

        public static void Init_Table(Goblin_Spec[] list) {
            babamama_table = new();
            babaonly_table = new();
            mamaonly_table = new();
            for (int i = 0; i < list.Length; i++) {
                var spec = list[i];

                if (spec.other.baba != "" && spec.other.mama != "") {
                    babamama_table[(spec.other.baba, spec.other.mama)] = spec;
                }
                else if (spec.other.baba != "") {
                    babaonly_table[spec.other.baba] = spec;
                }
                else if (spec.other.mama != "") {
                    mamaonly_table[spec.other.mama] = spec;
                }
            }
        }

        public static bool Try_Find_Babamama(string baba, string mama, out Goblin_Spec spec) {
            

            if (baba != "" && mama != "") {
                if (babamama_table.TryGetValue((baba, mama), out spec)) {
                    return true;
                }
            }
            if (baba != "") {
                if (babaonly_table.TryGetValue(baba, out spec)) {
                    return true;
                }
            }
            if (mama != "") {
                if (mamaonly_table.TryGetValue(mama, out spec)) {
                    return true;
                }
            }

            spec = default;
            return false;
        }

    }
}