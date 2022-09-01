using System;
using TMPro;

namespace UniRx
{
    public static partial class UnityUIComponentExtensions
    {
        public static IDisposable SubscribeToText(this IObservable<string> source, TextMeshPro text) {
            return source.SubscribeWithState(text, (x, t) => t.text = x);
        }

        public static IDisposable SubscribeToText<T>(this IObservable<T> source, TextMeshPro text) {
            return source.SubscribeWithState(text, (x, t) => t.text = x.ToString());
        }

        public static IDisposable SubscribeToText<T>(this IObservable<T> source, TextMeshPro text, Func<T, string> selector) {
            return source.SubscribeWithState2(text, selector, (x, t, s) => t.text = s(x));
        }
        public static IDisposable SubscribeToText(this IObservable<string> source, TextMeshProUGUI text) {
            return source.SubscribeWithState(text, (x, t) => t.text = x);
        }

        public static IDisposable SubscribeToText<T>(this IObservable<T> source, TextMeshProUGUI text) {
            return source.SubscribeWithState(text, (x, t) => t.text = x.ToString());
        }

        public static IDisposable SubscribeToText<T>(this IObservable<T> source, TextMeshProUGUI text, Func<T, string> selector) {
            return source.SubscribeWithState2(text, selector, (x, t, s) => t.text = s(x));
        }

        public static IDisposable SubscribeToText(this IObservable<string> source, TMP_Text text) {
            return source.SubscribeWithState(text, (x, t) => t.text = x);
        }

        public static IDisposable SubscribeToText<T>(this IObservable<T> source, TMP_Text text) {
            return source.SubscribeWithState(text, (x, t) => t.text = x.ToString());
        }

        public static IDisposable SubscribeToText<T>(this IObservable<T> source, TMP_Text text, Func<T, string> selector) {
            return source.SubscribeWithState2(text, selector, (x, t, s) => t.text = s(x));
        }
    }
}