using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Runtime.Serialization;

namespace JojoLabs.SchedulerService.Cron
{
    /// <summary>
    /// Crontab field implementation.
    /// </summary>
    /// <seealso cref="System.Runtime.Serialization.IObjectReference"/>
    [Serializable]
    public sealed class CrontabFieldImpl : IObjectReference
    {
        /// <summary>
        /// The <see cref="CrontabFieldImpl">crontab field</see> representing minutes.
        /// </summary>
        public static readonly CrontabFieldImpl Minute = new CrontabFieldImpl(CrontabFieldKind.Minute, 0, 59, null);

        /// <summary>
        /// The <see cref="CrontabFieldImpl">crontab field</see> representing hours.
        /// </summary>
        public static readonly CrontabFieldImpl Hour = new CrontabFieldImpl(CrontabFieldKind.Hour, 0, 23, null);

        /// <summary>
        /// The <see cref="CrontabFieldImpl">crontab field</see> representing days.
        /// </summary>
        public static readonly CrontabFieldImpl Day = new CrontabFieldImpl(CrontabFieldKind.Day, 1, 31, null);

        /// <summary>
        /// The <see cref="CrontabFieldImpl">crontab field</see> representing months.
        /// </summary>
        public static readonly CrontabFieldImpl Month = new CrontabFieldImpl(CrontabFieldKind.Month, 1, 12,
            new[]
            {
                "January", "February", "March", "April",
                "May", "June", "July", "August",
                "September", "October", "November",
                "December"
            });

        /// <summary>
        /// The <see cref="CrontabFieldImpl">crontab field</see> representing days of week.
        /// </summary>
        public static readonly CrontabFieldImpl DayOfWeek = new CrontabFieldImpl(CrontabFieldKind.DayOfWeek, 0, 6,
            new[]
            {
                "Sunday", "Monday", "Tuesday",
                "Wednesday", "Thursday", "Friday",
                "Saturday"
            });

        /// <summary>
        /// The list of <see cref="CrontabFieldImpl">crontab field</see>.
        /// </summary>
        private static readonly CrontabFieldImpl[] FieldByKind = { Minute, Hour, Day, Month, DayOfWeek };

        /// <summary>
        /// The comparer for culture-string sensitive.
        /// </summary>
        private static readonly CompareInfo Comparer = CultureInfo.InvariantCulture.CompareInfo;

        /// <summary>
        /// The delimiter comma.
        /// </summary>
        private static readonly char[] Comma = { ',' };

        /// <summary>
        /// The Crontab field kind.
        /// </summary>
        private readonly CrontabFieldKind _kind;

        /// <summary>
        /// The maximum value.
        /// </summary>
        private readonly int _maxValue;

        /// <summary>
        /// The minimum value.
        /// </summary>
        private readonly int _minValue;

        /// <summary>
        /// The names.
        /// </summary>
        private readonly string[] _names;

        /// <summary>
        /// Initializes a new instance of the <see cref="CrontabFieldImpl"/> class.
        /// </summary>
        /// <param name="kind">The kind.</param>
        /// <param name="minValue">The minimum value.</param>
        /// <param name="maxValue">The maximum value.</param>
        /// <param name="names">The names.</param>
        private CrontabFieldImpl(CrontabFieldKind kind, int minValue, int maxValue, string[] names)
        {
            Debug.Assert(Enum.IsDefined(typeof(CrontabFieldKind), kind), "Unknwown CrontabFieldKind");
            Debug.Assert(minValue >= 0, "Minimum value must be over 0.");
            Debug.Assert(maxValue >= minValue, "Minimum value must be over minimum value.");
            Debug.Assert(names == null || names.Length == (maxValue - minValue + 1));

            _kind = kind;
            _minValue = minValue;
            _maxValue = maxValue;
            _names = names;
        }

        /// <summary>
        /// Gets the kind.
        /// </summary>
        /// <value>The kind.</value>
        public CrontabFieldKind Kind => _kind;

        /// <summary>
        /// Gets the minimum value.
        /// </summary>
        /// <value>The minimum value.</value>
        public int MinValue => _minValue;

        /// <summary>
        /// Gets the maximum value.
        /// </summary>
        /// <value>The maximum value.</value>
        public int MaxValue => _maxValue;

        /// <summary>
        /// Gets the value count.
        /// </summary>
        /// <value>The value count.</value>
        public int ValueCount
        {
            get { return _maxValue - _minValue + 1; }
        }

        #region IObjectReference Members

        /// <summary>
        /// Returns the real object that should be deserialized, rather than the object that the serialized stream specifies.
        /// </summary>
        /// <param name="context">
        /// The <see cref="T:System.Runtime.Serialization.StreamingContext"></see> from which the current object is deserialized.
        /// </param>
        /// <returns>Returns the actual object that is put into the graph.</returns>
        object IObjectReference.GetRealObject(StreamingContext context)
        {
            return FromKind(Kind);
        }

        #endregion IObjectReference Members

        /// <summary>
        /// Gets a Crontab field implementation from the kind.
        /// </summary>
        /// <param name="kind">The kind.</param>
        /// <returns>The Crontab field implementation.</returns>
        /// <exception cref="ArgumentException">Unknwon kind.</exception>
        public static CrontabFieldImpl FromKind(CrontabFieldKind kind)
        {
            if (!Enum.IsDefined(typeof(CrontabFieldKind), kind))
            {
                throw new ArgumentException(string.Format(
                    "Invalid crontab field kind. Valid values are {0}.",
                    string.Join(", ", Enum.GetNames(typeof(CrontabFieldKind)))), nameof(kind));
            }

            return FieldByKind[(int)kind];
        }

        /// <summary>
        /// Formats the specified field.
        /// </summary>
        /// <param name="field">The field.</param>
        /// <param name="writer">The writer.</param>
        /// <param name="noNames">If set to <c>true</c> [no names].</param>
        /// <exception cref="ArgumentNullException">Field or writer is null.</exception>
        public void Format(CrontabField field, TextWriter writer, bool noNames)
        {
            if (field == null)
            {
                throw new ArgumentNullException(nameof(field));
            }

            if (writer == null)
            {
                throw new ArgumentNullException(nameof(writer));
            }

            var next = field.GetFirst();
            var count = 0;

            while (next != -1)
            {
                var first = next;
                int last;

                do
                {
                    last = next;
                    next = field.Next(last + 1);
                } while (next - last == 1);

                if (count == 0 && first == _minValue && last == _maxValue)
                {
                    writer.Write('*');
                    return;
                }

                if (count > 0)
                    writer.Write(',');

                if (first == last)
                {
                    FormatValue(first, writer, noNames);
                }
                else
                {
                    FormatValue(first, writer, noNames);
                    writer.Write('-');
                    FormatValue(last, writer, noNames);
                }

                count++;
            }
        }

        /// <summary>
        /// Formats the value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="writer">The writer.</param>
        /// <param name="noNames">If set to <c>true</c> [no names].</param>
        private void FormatValue(int value, TextWriter writer, bool noNames)
        {
            Debug.Assert(writer != null);

            if (noNames || _names == null)
            {
                if (value >= 0 && value < 100)
                {
                    FastFormatNumericValue(value, writer);
                }
                else
                {
                    writer.Write(value.ToString(CultureInfo.InvariantCulture));
                }
            }
            else
            {
                var index = value - _minValue;
                writer.Write((string)_names[index]);
            }
        }

        /// <summary>
        /// Formats numeric value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="writer">The writer.</param>
        private static void FastFormatNumericValue(int value, TextWriter writer)
        {
            Debug.Assert(value >= 0 && value < 100);
            Debug.Assert(writer != null);

            if (value >= 10)
            {
                writer.Write((char)('0' + (value / 10)));
                writer.Write((char)('0' + (value % 10)));
            }
            else
            {
                writer.Write((char)('0' + value));
            }
        }

        /// <summary>
        /// Parses the specified string.
        /// </summary>
        /// <param name="str">The string.</param>
        /// <param name="acc">The acc.</param>
        /// <exception cref="ArgumentNullException">An accumulator value cannot be null.</exception>
        public void Parse(string str, CrontabFieldAccumulator acc)
        {
            if (acc == null)
            {
                throw new ArgumentNullException(nameof(acc));
            }

            if (string.IsNullOrEmpty(str))
            {
                return;
            }

            try
            {
                InternalParse(str, acc);
            }
            catch (FormatException e)
            {
                ThrowParseException(e, str);
            }
        }

        /// <summary>
        /// Throws the parse exception.
        /// </summary>
        /// <param name="innerException">The inner exception.</param>
        /// <param name="str">The string.</param>
        /// <exception cref="FormatException">The format exception.</exception>
        private static void ThrowParseException(Exception innerException, string str)
        {
            Debug.Assert(str != null);
            Debug.Assert(innerException != null);

            throw new FormatException(string.Format("'{0}' is not a valid crontab field expression.", str), innerException);
        }

        /// <summary>
        /// Internals the parse.
        /// </summary>
        /// <param name="str">The string.</param>
        /// <param name="acc">The acc.</param>
        /// <exception cref="FormatException">A crontab field value cannot be empty.</exception>
        private void InternalParse(string str, CrontabFieldAccumulator acc)
        {
            Debug.Assert(str != null);
            Debug.Assert(acc != null);

            if (str.Length == 0)
                throw new FormatException("A crontab field value cannot be empty.");

            // Next, look for a list of values (e.g. 1,2,3).
            var commaIndex = str.IndexOf(",", StringComparison.Ordinal);

            if (commaIndex > 0)
            {
                foreach (var token in str.Split(Comma))
                    InternalParse(token, acc);
            }
            else
            {
                var every = 1;

                // Look for stepping first (e.g. */2 = every 2nd).
                var slashIndex = str.IndexOf("/", StringComparison.Ordinal);

                if (slashIndex > 0)
                {
                    every = int.Parse(str.Substring(slashIndex + 1), CultureInfo.InvariantCulture);
                    str = str.Substring(0, slashIndex);
                }

                // Next, look for wildcard (*).
                if (str.Length == 1 && str[0] == '*')
                {
                    acc(-1, -1, every);
                    return;
                }

                // Next, look for a range of values (e.g. 2-10).
                var dashIndex = str.IndexOf("-", StringComparison.Ordinal);

                if (dashIndex > 0)
                {
                    var first = ParseValue(str.Substring(0, dashIndex));
                    var last = ParseValue(str.Substring(dashIndex + 1));

                    acc(first, last, every);
                    return;
                }

                // Finally, handle the case where there is only one number.
                var value = ParseValue(str);

                if (every == 1)
                {
                    acc(value, value, 1);
                }
                else
                {
                    Debug.Assert(every != 0);

                    acc(value, _maxValue, every);
                }
            }
        }

        /// <summary>
        /// Parses the value.
        /// </summary>
        /// <param name="str">The string.</param>
        /// <returns></returns>
        /// <exception cref="FormatException">A crontab field value cannot be empty.</exception>
        private int ParseValue(string str)
        {
            Debug.Assert(str != null);

            if (str.Length == 0)
            {
                throw new FormatException("A crontab field value cannot be empty.");
            }

            var firstChar = str[0];
            if (firstChar >= '0' && firstChar <= '9')
            {
                return int.Parse(str, CultureInfo.InvariantCulture);
            }

            if (_names == null)
            {
                throw new FormatException(string.Format(
                    "'{0}' is not a valid value for this crontab field. It must be a numeric value between {1} and {2} (all inclusive).",
                    str, _minValue, _maxValue));
            }

            for (var i = 0; i < _names.Length; i++)
            {
                if (Comparer.IsPrefix(_names[i], str, CompareOptions.IgnoreCase))
                {
                    return i + _minValue;
                }
            }

            throw new FormatException(string.Format(
                "'{0}' is not a known value name. Use one of the following: {1}.",
                str, string.Join(", ", _names)));
        }
    }
}
