using System;
using System.Collections;
using System.Linq;

namespace Scraper.LightShot
{
    public class Indexer : IEnumerator, IDisposable
    {
        const string Chars = "0123456789abcdefghijklmnopqrstuvwxyz";
        private CharEnumerator[] _enumerators;

        /// <inheritdoc />
        public Indexer(string start)
        {
            if (string.IsNullOrEmpty(start))
                throw new ArgumentNullException(nameof(start), "Must have contain only letters and digits.");

            var check = new string(start.ToLower().ToCharArray().Where(char.IsLetterOrDigit).ToArray());
            if (!string.Equals(check, start, StringComparison.InvariantCultureIgnoreCase))
                throw new ArgumentException("Must have contain only letters and digits.", nameof(start));

            var length = check.Length;
            var enumerators = new CharEnumerator[length];
            for (var i = 0; i < length; i++)
            {
                enumerators[i] = Chars.GetEnumerator();
            }

            MoveTo(check);
        }

        private void MoveTo(string value)
        {
            var chars = value.ToCharArray();
            for (var i = 0; i < chars.Length; i++)
            {
                var c = chars[i];
                var e = _enumerators[i];

                // Move each iterator to requested char.
                while (e.Current != c && e.MoveNext()) { }
            }
        }

        /// <inheritdoc />
        public bool MoveNext()
        {
            if (_enumerators == null)
                return false;

            for (var i = _enumerators.Length - 1; i >= 0; i--)
            {
                var e = _enumerators[i];
                if (e.MoveNext())
                    break;

                e.Reset();
                if (i == 0)
                    return false;
            }

            UpdateCurrent();
            return true;
        }

        /// <inheritdoc />
        public void Reset()
        {
            foreach (var enumerator in _enumerators)
            {
                enumerator.Reset();
            }
        }

        private void UpdateCurrent()
        {
            Current = new string(_enumerators.Select(x => x.Current).ToArray());
        }


        /// <inheritdoc />
        public object Current { get; private set; }


        /// <inheritdoc />
        public void Dispose()
        {
            foreach (var enumerator in _enumerators)
            {
                enumerator.Dispose();
            }

            _enumerators = null;
            Current = null;
        }
    }
}