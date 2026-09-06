namespace GoogleSheetsDemo.Validation
{
    /// <summary>
    /// A single input rule violation: which field failed and why.
    /// Pure data - the UI decides how to present it.
    /// </summary>
    public class ValidationFailure
    {
        public ValidationFailure(string field, string message)
        {
            Field = field;
            Message = message;
        }

        /// <summary>Field or row label the failure belongs to (e.g. "Name", "Row 3").</summary>
        public string Field { get; private set; }

        /// <summary>Human readable description of the rule that was broken.</summary>
        public string Message { get; private set; }
    }
}
