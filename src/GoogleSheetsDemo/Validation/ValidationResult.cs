using System.Collections.Generic;
using System.Text;

namespace GoogleSheetsDemo.Validation
{
    /// <summary>
    /// Outcome of validating one input or a batch of rows: zero failures means
    /// valid. Carries every failure at once so the user can fix them in a
    /// single pass instead of one dialog per field.
    /// </summary>
    public class ValidationResult
    {
        private readonly List<ValidationFailure> _failures = new List<ValidationFailure>();

        public bool IsValid
        {
            get { return _failures.Count == 0; }
        }

        public IList<ValidationFailure> Failures
        {
            get { return _failures; }
        }

        public void AddFailure(string field, string message)
        {
            _failures.Add(new ValidationFailure(field, message));
        }

        /// <summary>
        /// Builds the text for a message box: a single failure shows its message
        /// alone, multiple failures are listed one per line as "- Field: message".
        /// </summary>
        public string ToDialogText()
        {
            if (_failures.Count == 0)
            {
                return "";
            }

            if (_failures.Count == 1)
            {
                return _failures[0].Message;
            }

            var builder = new StringBuilder();
            for (int i = 0; i < _failures.Count; i++)
            {
                if (i > 0)
                {
                    builder.AppendLine();
                }
                builder.Append("- ");
                builder.Append(_failures[i].Field);
                builder.Append(": ");
                builder.Append(_failures[i].Message);
            }
            return builder.ToString();
        }
    }
}
