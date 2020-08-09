using System;
using System.Collections.Generic;
using System.Linq;

namespace WslManager
{
    public sealed class DistroInfoList
    {
        public DistroInfoList(string expression)
        {
            if (expression == null)
                throw new ArgumentNullException(nameof(expression));

            _distroListExpression = expression;

            var lines = _distroListExpression.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            var list = new List<DistroInfo>(Math.Max(0, lines.Length - 1));

            foreach (var eachLine in lines)
            {
                var items = eachLine.Trim().Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);

                if (3 <= items.Length && items.Length <= 4)
                {
                    var info = new DistroInfo();

                    if (items[0] == "*")
                    {
                        items = items.Skip(1).ToArray();
                        info.IsDefault = true;
                    }

                    info.DistroName = items[0];
                    info.DistroStatus = items[1];
                    info.WSLVersion = items[2];

                    if (!string.Equals(info.DistroName, "NAME", StringComparison.OrdinalIgnoreCase) &&
                        !string.Equals(info.DistroStatus, "STATE", StringComparison.OrdinalIgnoreCase) &&
                        !string.Equals(info.WSLVersion, "VERSION", StringComparison.OrdinalIgnoreCase))
                    {
                        list.Add(info);
                    }
                }
            }

            _list = list.ToArray();
        }

        private readonly string _distroListExpression;
        private DistroInfo[] _list;

        public IEnumerable<DistroInfo> DistroList
            => _list;
    }
}
