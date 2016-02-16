
using System;
using NDesk.Options;

namespace ManyConsole
{

    // from https://github.com/chakrit/ndesk-options-mirror/blob/master/src/NDesk.Options/NDesk.Options/Options.cs
    sealed class HideableActionOption: Option, IHidableOption {
        Action<OptionValueCollection> action;

        public HideableActionOption(string prototype, string description, int count, Action<OptionValueCollection> action, bool isHidden = false)
            : base(prototype, description, count) {
            if (action == null)
                throw new ArgumentNullException("action");
            this.action = action;
            IsHidden = isHidden;
            }

        protected override void OnParseComplete(OptionContext c) {
            action(c.OptionValues);
        }

        public bool IsHidden { get; }
    }

    sealed class HideableActionOption<T>: Option, IHidableOption {
        Action<T> action;

        public HideableActionOption(string prototype, string description, Action<T> action, bool isHidden = false)
            : base(prototype, description, 1) {
            if (action == null)
                throw new ArgumentNullException("action");
            this.action = action;
            IsHidden = isHidden;
            }

        protected override void OnParseComplete(OptionContext c) {
            action(Parse<T>(c.OptionValues[0], c));
        }

        public bool IsHidden { get; }
    }

    sealed class HideableActionOption<TKey, TValue>: Option, IHidableOption {
        OptionAction<TKey, TValue> action;

        public HideableActionOption(string prototype, string description, OptionAction<TKey, TValue> action, bool isHidden = false)
            : base(prototype, description, 2) {
            if (action == null)
                throw new ArgumentNullException("action");
            this.action = action;
            IsHidden = isHidden;
            }

        protected override void OnParseComplete(OptionContext c) {
            action(
                    Parse<TKey>(c.OptionValues[0], c),
                    Parse<TValue>(c.OptionValues[1], c));
        }

        public bool IsHidden { get; }
    }

    internal interface IHidableOption {
        bool IsHidden { get; }
    }
}

