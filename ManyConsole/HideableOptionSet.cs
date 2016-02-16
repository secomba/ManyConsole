using System;
using System.IO;
using System.Linq;
using NDesk.Options;

namespace ManyConsole {
    public class HideableOptionSet: OptionSet {
        public HideableOptionSet Add<T>(string prototype, string description, Action<T> action, bool hidden)
        {
            return Add(new HideableActionOption<T>(prototype, description, action, hidden)) as HideableOptionSet;
        }

        public HideableOptionSet Add<TKey, TValue>(string prototype, string description, OptionAction<TKey, TValue> action, bool hidden)
        {
            return Add(new HideableActionOption<TKey, TValue>(prototype, description, action, hidden)) as HideableOptionSet;
        }

        public new void WriteOptionDescriptions(TextWriter o)
        {
            // generate a list of hidden options
            var originalOptions = this.Where(it => true).ToList();
            var visibleOptions = originalOptions.Where(it => !this.IsOptionHidden(it)).ToList();

            this.ClearItems();
            this.Clear();

            visibleOptions.ForEach(it => this.Add(it));

            // .. print all other options and .. 
            base.WriteOptionDescriptions(o);

            this.ClearItems();
            this.Clear();

            originalOptions.ForEach(it => this.Add(it));
        }

        private bool IsOptionHidden(Option option)
        {
            var isHidden = option.GetType().GetProperty("IsHidden")?.GetValue(option, null);
            return isHidden is bool && (bool) isHidden;
        }
    }
}
