namespace SpiceNetlist.SpiceObjects.Parameters
{
    public abstract class SingleParameter : Parameter
    {
        private string rawString;

        public SingleParameter(string rawString)
        {
            this.rawString = rawString;
        }

        public override string Image => this.rawString;
    }
}
