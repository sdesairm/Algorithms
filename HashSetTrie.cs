using System.Collections.Generic;

namespace SDesaiRM.Algorithms
{
    public class HashSetTrie
    {

        public HashSetTrie()
        {
            Characters = new Dictionary<char, HashSetTrie>();
        }

        private Dictionary<char,HashSetTrie> Characters { get; }

        private List<int> Suffix { get; } = new List<int>();
        private List<int> TerminatedStrings { get; } = new List<int>();

        private bool StringTerminated { get; set; }

        public void InsertString(string s, int refIndex)
        {
            if (string.IsNullOrEmpty(s))
            {
                StringTerminated = true;
                TerminatedStrings.Add(refIndex);
                return;
            }

            if (!Characters.ContainsKey(s[0]))
            {
                Characters[s[0]] = new HashSetTrie();
            }

            Characters[s[0]].Suffix.Add(refIndex);
            Characters[s[0]].InsertString(s.Substring(1), refIndex);
        }

        public bool Search(string txt)
        {
            var ss = this;
            var j = 0;
            while (j < txt.Length)
            {
                if (ss.StringTerminated)
                    return true;
                if (ss.Characters.ContainsKey(txt[j]))
                {
                    ss = ss.Characters[txt[j]];
                    j++;
                }
                else break;
            }

            if (j != txt.Length) return false;
            if (null != ss.Suffix)
            {
                return ss.Suffix.Count > 0;
            }

            return false;
        }

        public List<int> Contains(string txt)
        {
            List<int> rtn = null;
            var x1 = StartsWith(txt);
            if (x1!=null)
            {
                rtn = x1;
            }
            foreach (var trie in Characters.Values)
            {
                var x = trie?.Contains(txt);
                if (x == null) continue;
                if (null != rtn)
                {
                    rtn.AddRange(x);
                }
                else
                    rtn = x;
            }
            return rtn;
        }

        public List<int> StartsWith(string txt)
        {
            var ss = this;
            var j = 0;
            while (j < txt.Length)
            {
                if (ss.Characters.ContainsKey(txt[j]))
                {
                    ss = ss.Characters[txt[j]];
                    j++;
                }
                else break;
            }

            if (j != txt.Length) return null;
            if (null != ss.Suffix)
            {
                return ss.Suffix;
            }

            return null;
        }
    }
}
