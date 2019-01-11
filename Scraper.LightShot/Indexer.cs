using System;
using System.Collections;
using System.Linq;

namespace Scraper.LightShot
{
    /// <summary>
    /// Indexer for LightShot image hosting.
    /// </summary>
    public class Indexer : IEnumerator, IDisposable
    {
        const string Chars = "0123456789abcdefghijklmnopqrstuvwxyz";
        private CharEnumerator[] _enumerators;

        public Indexer(int count)
        : this(new string(Chars[0], count))
        { }

        /// <inheritdoc />
        public Indexer(string start)
        {
            if (string.IsNullOrEmpty(start))
                throw new ArgumentNullException(nameof(start), "Must have contain only letters and digits.");

            var check = new string(start.ToLower().ToCharArray().Where(char.IsLetterOrDigit).ToArray());
            if (!string.Equals(check, start, StringComparison.InvariantCultureIgnoreCase))
                throw new ArgumentException("Must have contain only letters and digits.", nameof(start));

            var length = check.Length;
            _enumerators = new CharEnumerator[length];
            for (var i = 0; i < length; i++)
            {
                var e = Chars.GetEnumerator();
                _enumerators[i] = e;
                e.MoveNext();
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
                e.Reset();
                e.MoveNext();

                // Move each iterator to requested char.
                while (e.Current != c && e.MoveNext()) { }
            }
            UpdateCurrent();
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
                e.MoveNext();

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
                enumerator.MoveNext();
            }
        }

        private void UpdateCurrent()
        {
            Current = new string(_enumerators.Select(x => x.Current).ToArray());
        }


        public string Current { get; private set; }

        /// <inheritdoc />
        object IEnumerator.Current => Current;


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