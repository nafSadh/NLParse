using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using System.Text;

namespace NLParseAP
{
    /// <summary>
    /// Translated into C# from Original Java Code by Nazmul Hasnat Arka & Sadre-Ala Parvez
    /// Enhanced and Modified by nafSadh (sadh@nafsadh.org), Qudrat-E-Alahy RATUL, Samiul Hoque SOURAV & Rushdi SHAMS
    /// Functioanlity added for parsing English and preparing data especially in order to produce machine intelligible RDF
    /// </summary>
    class PredictiveBacktrackingNLParser
    {
        #region filepath
        /// <summary>
        /// Filepath to ParseTable.XML
        /// </summary>
        public String f_parseTable;
        /// <summary>
        /// Filepath to Lexicon (Conj Aux Prep Pron Art) XML
        /// </summary>
        public String f_lexicon_cappa;
        /// <summary>
        /// Filepath to Lexicon (Proper Nouns) XML
        /// </summary>
        public String f_lexicon_name;
        /// <summary>
        ///  Filepath fo Lexicon (Verb Adv Noun Adj) XML
        /// </summary>
        public String f_lexicon_vana;
        #endregion
        #region fields
        /// <summary>
        /// the Sentence being parsed
        /// </summary>
        private String sentence;
        /// <summary>
        /// words in sentence
        /// </summary>
        private String[] words;
        /// <summary>
        /// Parts of Speech of words
        /// </summary>
        private String[] pos;
        /// <summary>
        /// tokenised input symbols
        /// </summary>
        private String[] tokens;
        /// <summary>
        /// Rules produced in term of Parsing
        /// </summary>
        private String[] rules;
        /// <summary>
        /// backtrack options
        /// </summary>
        private String[] options;
        /// <summary>
        /// Predictive Parse Stack
        /// </summary>
        Stack<String> stck = new Stack<String>();
        /// <summary>
        /// Stack of Stack
        /// </summary>
        Stack<String>[] stckOfStck = new Stack<String>[90];
        private int[] wordcount;
        private int[] stacklength;
        private int[] num;
        /// <summary>
        /// length of sentence as wordcount
        /// </summary>
        private int len;
        /// <summary>
        /// number of rulz
        /// </summary>
        private int noOfrules;
        private int currentRule;
        private int posPrep = -2;
        private int posArt = -2;
        /// <summary>
        /// index of current word in Q for being parsed
        /// </summary>
        int currentword;
        private int count;
        private bool finished;
        /// <summary>
        /// Position of first preposition
        /// </summary>
        public int PosPrep
        {
            get { return posPrep; }
        }
        /// <summary>
        /// position of an Article
        /// </summary>
        public int PosArt
        {
            get { return posArt; }
        }
        /// <summary>
        /// Initialize parser with XML datasources to deafualt locations
        /// </summary>
        public PredictiveBacktrackingNLParser()
        {
            f_parseTable = "parseTable.xml";
            f_lexicon_cappa = "cappa.xml";
            f_lexicon_name = "name.xml";
            f_lexicon_vana = "vana.xml";
        }
        /// <summary>
        /// initialize parser showing paths for ParseTable and Lexicon XML files
        /// </summary>
        /// <param name="parseTableXML">Filepath to ParseTable.XML</param>
        /// <param name="lexiconCAPPA">Filepath to Lexicon (Conj Aux Prep Pron Art) XML</param>
        /// <param name="lexiconName">Filepath to Lexicon (Proper Noun) XML</param>
        /// <param name="lexiconVANA">Filepath to Lexicon (Verb Adv Noun Adj) XML</param>
        public PredictiveBacktrackingNLParser(
            String parseTableXML, 
            String lexiconCAPPA, 
            String lexiconName,
            String lexiconVANA)
        {
            f_parseTable = parseTableXML;
            f_lexicon_cappa = lexiconCAPPA;
            f_lexicon_name = lexiconName;
            f_lexicon_vana = lexiconVANA;
        }
        /// <summary>
        /// Print in default color
        /// </summary>
        /// <param name="str">String to print</param>
        private void print(String str)
        {
            //Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(str);
            //Console.ResetColor();
        }
        /// <summary>
        /// Highlight and Print in blue
        /// </summary>
        /// <param name="str">String to print</param>
        private void printb(String str)
        {
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine(str);
            Console.ResetColor();
        }
        /// <summary>
        /// Most highlited string 
        /// </summary>
        /// <param name="str">String to print</param>
        private void printc(String str)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine(str);
            Console.ResetColor();
        }
        /// <summary>
        /// Print error in red
        /// </summary>
        /// <param name="str">String to print</param>
        private void printr(String str)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(str);
            Console.ResetColor();
        }
        #endregion
        /// <summary>
        /// Finds the Part of Speech of the passed word from 3 basic lexicon
        /// </summary>
        /// <param name="word">Word of which POS is to be searched</param>
        /// <returns></returns>
        String findPOS(String word, int i)
        {
            String pos="";
            String wordl = word;
            wordl = wordl.ToLower();
            if (word == "I") return "pron";
            ///CAPPA
            XDocument xmlDoc = XDocument.Load(f_lexicon_cappa);
            var q = from c in xmlDoc.Descendants("word") 
                    where c.Attribute("value").Value == wordl
                    select (string)c.Attribute("pos");
            foreach (string cappa in q){
                pos = cappa; if (cappa != null)return pos;
            }
            ///Name
            xmlDoc = XDocument.Load(f_lexicon_name);
            var r = from c in xmlDoc.Descendants("word")
                    where c.Attribute("value").Value == word
                    select (string)c.Attribute("pos");
            foreach (string name in r)
            {
                pos = name; if (name!= null) return pos;
            }
            ///VANA
            xmlDoc = XDocument.Load(f_lexicon_vana);
            var s = from c in xmlDoc.Descendants("word")
                    where c.Attribute("value").Value == wordl
                    || (c.Attribute("value").Value == wordl.Remove(wordl.Length - 1) && (wordl.ToCharArray()[wordl.Length - 1] == 's'))// if ends with appended s or d
                    || (c.Attribute("value").Value == wordl.Remove(wordl.Length - 2) && wordl.ToCharArray()[wordl.Length - 2] == 'e' && (wordl.ToCharArray()[wordl.Length - 1] == 's' || wordl.ToCharArray()[wordl.Length - 1] == 'd'))// if ends with appended es or ed
                    select (string)c.Attribute("pos")+" "+(string)c.Attribute("value");
            foreach (string vana in s)
            {
                if (vana != null)
                {   
                    pos = vana.Split(' ')[0];
                    if (pos == "verb") words[i] = vana.Split(' ')[1];//update word exluding trailing s or es or ed
                    return pos;
                }
            }
            ///not in lexicon? then probably it is a proper noun. let us call unknown proper noun as iname
            pos = "iname";
            return pos;
        }

        /// <summary>
        /// est POS function - don't use it, u damn!!!
        /// </summary>
        /// <param name="word"></param>
        /// <returns></returns>
        String getPos(String word)
        {
            switch (word.ToLower())
            {
                case "i": return "pron";
                case "you": return "pron";
                case "eat": return "verb";
                case "based": return "verb";
                case "go": return "verb";
                case "walk": return "verb";
                case "say": return "verb";
                case "play": return "verb";
                case "run": return "verb";
                case "rice": return "noun";
                case "chelsea": return "noun";
                case "club": return "noun";
                case "football": return "noun";
                case "english": return "adj";
                case "london": return "noun";
                case "shop": return "noun";
                case "goal": return "noun";
                case "money": return "noun";
                case "player": return "adj";
                case "roonie": return "noun";
                case "car": return "noun";
                case "manu": return "noun";
                case "very": return "adv";
                case "good": return "adj";
                case "nice": return "adj";
                case "professional": return "adj";
                case "west": return "adj";
                case "to": return "prep";
                case "a": return "art";
                case "an": return "art";
                case "the": return "art";
                case "and": return "conj";
                case "is": return "aux";
                case "are": return "aux";
                case "in": return "prep";
                default: return "";
            }
        }
        
        /// <summary>
        /// Initialize Parts of speech of words in the sentence
        /// </summary>
        /// <param name="Words">the array holding words</param>
        /// <returns>an array holding POS</returns>
        String[] initPos(String[] Words)
        {
            printb("init POS");
            String[] POS = new String[Words.Length];
            int i = 0;
            foreach (String word in Words)
            {
                POS[i] = findPOS(word, i);
                print(word+" is a "+POS[i++]);
            }
            return POS;
        }

        /// <summary>
        /// find the token for an input symbol word depending on its POS
        /// </summary>
        /// <param name="POS">POS of the word to find token of</param>
        /// <returns>token for the word</returns>
        String getToken(String POS)
        {
            switch (POS.ToLower())
            {
                case "iname":
                case "name":
                case "noun": return "a";

                case "pronoun":
                case "pron": return "b";

                case "verb": return "c";

                case "adjective":
                case "adj": return "d";
                
                case "adverb":
                case "adv": return "e";
                
                case "preposition":
                case "prep": return "f";

                case "conjunction":
                case "conj": return "g";

                case "article":
                case "art": return "h";
                case "degree":
                case "deg": return "i";

                case "gerund": return "j";
                
                case "auxiliary-verb":
                case "aux": return "k";
                case "nounverb": return "nv";
                default: return "";
            }
        }

        /// <summary>
        /// Initialize tokens for all word
        /// </summary>
        /// <param name="POSs">Array of POS</param>
        /// <returns>Array of tokens</returns>
        String[] initTokens(String[] POSs)
        {
            printb("init Token");
            String[] Tokens = new String[POSs.Length];
            int i = 0;
            foreach (String Pos in POSs) Tokens[i++] = getToken(Pos);
            bool hasVerb = false;
            for (int j = 0; j < i; j++)
            {
                if (Tokens[j] == "c" || Tokens[j] == "k") hasVerb = true;
                if (hasVerb == true && Tokens[j] == "nv") Tokens[j] = "a";
                else if (hasVerb == false && Tokens[j] == "nv") { Tokens[j] = "c"; hasVerb = true; }
            }
            return Tokens;
        }

        /// <summary>
        /// Insert production into the parse stack
        /// </summary>
        /// <param name="production"></param>
        private void addProduction(String production)
        {
            noOfrules++;
            stck.Pop();
            if (production != "z")
            {
                char[] ar = production.ToCharArray();
                int l = ar.Length;
                int m = l / 2;
                char temp;
                for (int i = 0; i < m; i++)
                {
                    temp = ar[i];
                    ar[i] = ar[l - 1 - i];
                    ar[l - i - 1] = temp;
                }
                for (int i = 0; i < l; i++)
                {
                    stck.Push(ar[i].ToString());
                }
            }
        }

        /// <summary>
        /// get the backtrack point if it is needed to backtrack
        /// </summary>
        /// <returns>int value denoting backtrack point</returns>
        private int getBacktrackpoint()
        {
            for (int i = noOfrules; i >= 0; i--)
            {
                if (options[i] != null)
                    return i;
                printb(i + "getting backtrack point");
            }
            return -1;
        }

        /// <summary>
        /// if there is to backtrack
        /// </summary>
        /// <returns></returns>
        private bool backtrack()
        {
            int position = getBacktrackpoint();
            for (int i = 0; i <= position; i++)
                //print(stckOfStck[i].Peek() + " stkofstk top");//~~
            if (position == -1)
            {
                printr("messed up "/*+Rules[NoOfrules--]+"..  "+Currentword+".."+stck.SeeTop()*/);
                return false;
            }
            for (int i = position + 1; i <= noOfrules; i++)
            {
                rules[i] = null;
            }
            noOfrules = position;
            String substitute;
            String temp = options[position];
            int p = temp.IndexOf(",");
            /* int destLength=stacklength[position];
             int presentlength=stck.seeLength();
             for(int i=presentlength;i>destLength;i--)
                 stck.pop();
             */
            stck = stckOfStck[position];

            if (p == -1)
            {
                rules[noOfrules] = rules[noOfrules].Substring(0, 1) + "->" + temp;
                print(temp + ">>>>>");
                options[position] = null;
                addProduction(temp);
                //   NoOfrules++;
            }
            else
            {
                String[] changes = temp.Split(',');
                rules[noOfrules] = rules[noOfrules].Substring(0, 1) + "->" + changes[0];
                options[position] = temp.Substring(p + 1);
                print(changes[0] + ">>>>>");
                addProduction(changes[0]);
                //  NoOfrules++;
            }

            currentword = wordcount[position];
            //addProduction(Rules[NoOfrules-1].substring(3));
            return true;
        }

        /// <summary>
        /// finds production by NonTerminal and terminal from PARSE-TABLE
        /// </summary>
        /// <param name="NonTerminal">NonTerminal of left hand side prod</param>
        /// <param name="terminal">terminal input</param>
        private void nonTerminal(String NonTerminal, String terminal)
        {
            //printb("Production: "+NonTerminal + " x " + terminal);
            //String[] Rules=new String[20];
            XDocument parseTable = XDocument.Load(f_parseTable);
            var nonTermNode = from entry in parseTable.Descendants("nonTerm")
                              where entry.Attribute("id").Value == NonTerminal
                              select entry;
            var productNode = from prod in nonTermNode.Descendants("production")
                              where prod.Attribute("input").Value == terminal
                              select (string)prod.Value;
            String production = "";
            foreach (String name in productNode) { production = name; printb("#"+NonTerminal+"*"+terminal+" = "+NonTerminal+"->"+production); }
            if (production == "error")
            {
                printb("!#! "+NonTerminal+"*"+terminal+" :test for backtrack ");
                backtrack();
                printb("retoken: "+tokens[currentword]);
            }
            /*  if(NonTerminal.equals("E")&&production.contains("B"))
                count++;
             *  System.out.println(NonTerminal+"test "+terminal);
             *  */
            else
            {
                int p = production.IndexOf(',');
                if (p != -1)
                {
                    String[] temp = production.Split(',');
                    printb("o#o Optional Production: "+production);
                    production = temp[0];
                    String sub = "";
                    for (int i = 1; i < temp.Length; i++)
                        sub = sub + "," + temp[i];
                    print(sub + " subb");
                    options[noOfrules] = sub.Substring(1);
                    print(noOfrules + "  rlno");
                }
                //System.out.println(production+"2");
                //stckOfStck[NoOfrules].Assign(stck.getall());
                String[] Bal = stck.ToArray();
                for (int i = 0; i < Bal.Length; i++)
                {
                    stckOfStck[noOfrules].Push(Bal[i]);
                    //print(Bal[i]);//print stk
                }
                print(stck.Peek() + "<><><>" + stckOfStck[0].Peek());
                rules[noOfrules] = NonTerminal + "->" + production;
                wordcount[noOfrules] = currentword;
                stacklength[noOfrules] = stck.Count();
                addProduction(production);
                //System.out.println(Rules[NoOfRulz]);                
            }
        }

        /// <summary>
        /// performs logic of PARSE
        /// </summary>
        private void parse2()
        {
            String top;
            char tp;
            //if(Currentword<len)

            while (currentword < len)
            {
                top = stck.Peek();
                tp = top.ToCharArray()[0];
                if (tp >= 'a' && tp <= 'z')
                {
                    if (top == tokens[currentword])
                    {
                        stck.Pop();
                        //   System.out.println(Tokens[Currentword]+" ****in parse2 is it rigft?");
                        currentword++;
                    }
                    else
                    {
                        if (!backtrack())
                            return;
                    }
                }
                else
                {
                    nonTerminal(top, tokens[currentword]);
                }

            }
            while (stck.Count > 0)
            {
                top = stck.Peek();
                print(top + "$ is TOP");
                tp = top.ToCharArray()[0];
                if (tp >= 'a' && tp <= 'z')
                {
                    if (backtrack())
                        parse2();
                    return;
                }
                else
                {
                    nonTerminal(top, "l");
                }
            }
        }

        /// <summary>
        /// count the terms
        /// </summary>
        /// <param name="Rul"></param>
        /// <param name="StartChar"></param>
        /// <param name="endChar"></param>
        /// <returns></returns>
        private int countTerm(String Rul, char StartChar, char endChar)
        {
            String temp = Rul.Substring(3);
            int count = 0;
            while (temp.Length > 0)
            {
                char ch = temp.ToCharArray()[0];
                if (ch >= StartChar && ch <= endChar && ch != 'z')
                    count++;
                temp = temp.Substring(1);
            }
            return count;
        }

        /// <summary>
        /// 
        /// </summary>
        private void AssignNum()
        {
            int start = 0;
            int end;
            int no = 1;
            int startCount = 0;
            int endCount = 0;
            int[] pos = new int[10];
            int point = 0;

            while (start < noOfrules)
            {
                if (rules[start].StartsWith("C"))
                {
                    pos[point] = start;
                    point++;
                }
                start++;
            }
            for (int i = 0; i < point - 1; i++)
            {
                start = pos[i];
                end = pos[i + 1];
                //count=0;
                endCount = startCount;
                for (int j = start; j < end; j++)
                {
                    endCount += countTerm(rules[j], 'a', 'z');
                }
                for (int k = startCount; k < endCount && k < len; k++)
                {
                    num[k] = no;
                }
                no++;
            }
        }

        /// <summary>
        /// Assign sentence to parser
        /// </summary>
        /// <param name="Sentence">sentence to assign</param>
        public void assign(String Sentence)
        {
            sentence = Sentence.Trim();
            words = sentence.Split(' ');
            len = words.Length;
            pos = initPos(words);
            num = new int[len];
            tokens = initTokens(pos);
            rules = new String[50];
            options = new String[50];
            wordcount = new int[50];
            stacklength = new int[50];
            for (int i = 0; i < 90; i++)
            {
                stckOfStck[i] = new Stack<String>();
            }
            for (int i = 0; i < 50; i++)
                options[i] = null;
            noOfrules = 0;
            currentRule = 0;
            currentword = 0;
            count = 0;
        }

        /// <summary>
        /// try to parse the input string from the start symbol
        /// 
        /// Start Symbol "A" is to parse sentence
        /// Start Symbol "C" is to parse Noun Phrase
        /// Start Symbol "F" is to parse Verb Phrase
        /// Start Symbol "J" is to parse preposition
        /// </summary>
        /// <param name="sentence">Sentence to parse</param>
        /// <param name="startSymbol">Start symbol from grammer to parse from</param>
        /// <returns>true if parse successfull else false</returns>
        public bool parse(String sentence, String startSymbol)
        {
            try
            {
                printc("$=" + sentence);
                assign(sentence);
                // int i=0;
                stck.Push(startSymbol);
                parse2();
                for (int i = 0; i < noOfrules; i++)
                    printc(rules[i]);
                AssignNum();
                posPrep = posOfPrep(1);
                posArt = posOfArt(1);
                return true;
            }
            catch (Exception ex)
            {
                printr("‼Sentence not parsed by grammar‼");
                printc(ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Finds position of first NounPhrase 
        /// </summary>
        /// <returns>int</returns>
        public int posOfNp()
        {
            for (int i = 1; i < noOfrules; i++)
                if (rules[i].StartsWith("C") && !rules[i].EndsWith("z"))
                    return i;
            return -1;
        }
        /// <summary>
        /// get position of jth NP
        /// </summary>
        /// <param name="j">j</param>
        /// <returns>int</returns>
        public int posOfNp(int j)
        {
            for (int i = 1; i < noOfrules; i++)
            {
                if (rules[i].StartsWith("C") && !rules[i].EndsWith("z"))
                {
                    j--;
                    if (j == 0) return i;
                }
            }
            return -1;
        }

        /// <summary>
        /// returns the position of jth NP which is a part of another VP
        /// </summary>
        /// <param name="j">J</param>
        /// <returns>int</returns>
        public int posOfNPinVP(int j)
        {
            for (int i = posOfVp(j); i < noOfrules; i++)
            {
                if (rules[i].StartsWith("C") && !rules[i].EndsWith("z"))
                {
                    j--;
                    if (j == 0) return i;
                }
            }
            return -1;
        }
        /// <summary>
        /// position of jth verb
        /// </summary>
        /// <param name="j">j</param>
        /// <returns>int</returns>
        public int posOfV(int j)
        {
            for (int i = 1; i < noOfrules; i++)
            {
                if (rules[i].StartsWith("M") && !rules[i].EndsWith("z"))
                {
                    j--;
                    if (j == 0) return i;
                }
            }
            return -1;
        }
        /// <summary>
        /// position of jth Verb Phrase
        /// </summary>
        /// <param name="j">j</param>
        /// <returns>int</returns>
        public int posOfVp(int j)
        {
            for (int i = 1; i < noOfrules; i++)
            {
                if (rules[i].StartsWith("F") && !rules[i].EndsWith("z"))
                {
                    j--;
                    if (j == 0) return i;
                }
            }
            return -1;
        }

        /// <summary>
        /// position of jth preposition
        /// </summary>
        /// <param name="j">j</param>
        /// <returns>int</returns>
        private int posOfPrep(int j)
        {
            for (int i = 1; i < noOfrules; i++)
            {
                if (rules[i].StartsWith("J") && !rules[i].EndsWith("z"))
                {
                    j--;
                    if (j == 0) return i;
                }
            }
            return -1;
        }

        /// <summary>
        /// position of jth article
        /// </summary>
        /// <param name="j">j</param>
        /// <returns>int</returns>
        private int posOfArt(int j)
        {
            for (int i = 1; i < noOfrules; i++)
            {
                if (rules[i].StartsWith("D") && !rules[i].EndsWith("z"))
                {
                    j--;
                    if (j == 0) return --i;
                }
            }
            return -1;
        }

        /// <summary>
        /// get first noun phrase
        /// </summary>
        /// <returns>String containing nounphrase</returns>
        public String getNp()
        {
            int pos = posOfNp();
            if (pos > 0)
            {
                int currentRule2 = pos;
                int count = 1;
                while (count > 0 && currentRule2 < noOfrules)
                { //System.out.println("here i am "+count);
                    count += countTerm(rules[currentRule2], 'A', 'Z');
                    count--;
                    currentRule2++;
                }
                return getParts(pos, currentRule2 - 1);
            }
            return null;
        }

        /// <summary>
        /// get jth NP (NounPhrase)
        /// </summary>
        /// <param name="j">j</param>
        /// <returns>string containing NP[j]</returns>
        public String getNp(int j)
        {
            int pos = posOfNp(j);
            if (pos > 0)
            {
                int currentRule2 = pos;
                int count = 1;
                while (count > 0 && currentRule2 < noOfrules)
                { //System.out.println("here i am "+count);
                    count += countTerm(rules[currentRule2], 'A', 'Z');
                    count--;
                    currentRule2++;
                }
                return getParts(pos, currentRule2 - 1);
            }
            return null;
        }

        /// <summary>
        /// get the jth verb
        /// </summary>
        /// <param name="j">j</param>
        /// <returns>verb as string</returns>
        public String getV(int j)
        {
            int pos = posOfV(j);
            if (pos > 0)
            {
                int currentRule2 = pos;
                int count = 1;
                while (count > 0 && currentRule2 < noOfrules)
                { //System.out.println("here i am "+count);
                    count += countTerm(rules[currentRule2], 'A', 'Z');
                    count--;
                    currentRule2++;
                }
                return getParts(pos, currentRule2 - 1);
            }
            return null;
        }

        /// <summary>
        /// get jth verbphrase
        /// </summary>
        /// <param name="j">j</param>
        /// <returns>VP as string</returns>
        public String getVp(int j)
        {
            int pos = posOfVp(j);
            if (pos > 0)
            {
                int currentRule2 = pos;
                int count = 1;
                while (count > 0 && currentRule2 < noOfrules)
                { //System.out.println("here i am "+count);
                    count += countTerm(rules[currentRule2], 'A', 'Z');
                    count--;
                    currentRule2++;
                }
                return getParts(pos, currentRule2 - 1);
            }
            return null;
        }

        /// <summary>
        /// probably an object
        /// </summary>
        /// <param name="j">J</param>
        /// <returns>returns the part of VP as NP trailing after the preposition</returns>
        public String getNpWithinVpAfterPrep(int j)
        {
            int pos = posOfVp(j);
            if (pos > 0)
            {
                int currentRule2 = pos;
                int count = 1;
                while (count > 0 && currentRule2 < noOfrules)
                { //System.out.println("here i am "+count);
                    count += countTerm(rules[currentRule2], 'A', 'Z');
                    count--;
                    currentRule2++;
                }
                return getParts(PosPrep+1, currentRule2 - 1);
            }
            return null;
        }
        /// <summary>
        /// probably an object
        /// </summary>
        /// <param name="j">J</param>
        /// <returns>returns the part of VP as NP trailing after the article</returns>
        public String getNpWithinVpAfterArticle(int j)
        {
            int pos = posOfVp(j);
            if (pos > 0)
            {
                int currentRule2 = pos;
                int count = 1;
                while (count > 0 && currentRule2 < noOfrules)
                { //System.out.println("here i am "+count);
                    count += countTerm(rules[currentRule2], 'A', 'Z');
                    count--;
                    currentRule2++;
                }
                return getParts(PosArt + 1, currentRule2 - 1);
            }
            return null;
        }
        /// <summary>
        /// probabl object
        /// </summary>
        /// <param name="j">J</param>
        /// <returns>returns the part of VP as NP trailing after the verb</returns>
        public String getNpWithinVpAfter(int j)
        {
            int pos = posOfVp(j);
            int posN = posOfNPinVP(j);
            if (pos > 0)
            {
                int currentRule2 = pos;
                int count = 1;
                while (count > 0 && currentRule2 < noOfrules)
                { //System.out.println("here i am "+count);
                    count += countTerm(rules[currentRule2], 'A', 'Z');
                    count--;
                    currentRule2++;
                }
                return getParts(posN, currentRule2 - 1);
            }
            return null;
        }

        /// <summary>
        /// probably predicate
        /// </summary>
        /// <param name="posV">position of VP</param>
        /// <param name="posP">position of prep</param>
        /// <returns>string containing VP upto prep</returns>
        private String getVerbaPrep(int posV, int posP)
        {
            if (posV > 0 && posP>0)
            {   /*
                int currentRule2 = posV;
                int count = 1;
                while (count > 0 && currentRule2 < noOfrules)
                { //System.out.println("here i am "+count);
                    count += countTerm(rules[currentRule2], 'A', 'Z');
                    count--;
                    currentRule2++;
                }*/
                return getParts(posV, posP);
            }
            return null;
        }

        /// <summary>
        /// probably predicate
        /// </summary>
        /// <param name="posV">position of VP</param>
        /// <param name="posA">position of article</param>
        /// <returns>string containing VP upto article</returns>
        private String getVerb2Art(int posV, int posA)
        {
            if (posV > 0 && posA > 0)
                return getParts(posV, posA);
            return null;
        }
        /// <summary>
        /// jth probable predicate
        /// </summary>
        /// <param name="j">J</param>
        /// <returns>predicate String</returns>
        public String getVerbion(int j)
        {
            int posV = posOfVp(j);
            if (PosPrep > 0) return getVerbaPrep(posV, PosPrep);
            else if (PosArt > 0) return getVerb2Art(posV, PosArt);
            else return getV(j);
        }
        /// <summary>
        /// Predicate parsed out of a sentence
        /// </summary>
        /// <returns>PRD:Predicate as string</returns>
        public String getPredicate()
        {
            return getVerbion(1);
        }
        /// <summary>
        /// Subject parsed from given sentence
        /// </summary>
        /// <returns>SUB:Subject as string</returns>
        public String getSubject()
        {
            return getNp(1);
        }
        /// <summary>
        /// Object parsed from a given sentece
        /// </summary>
        /// <returns>OBJ:Object as string</returns>
        public String getObject()
        {
            if (posOfNp(2) <= 0) return null;
            if (PosPrep > 0) return getNpWithinVpAfterPrep(1);
            else if (PosArt > 0) return getNpWithinVpAfterArticle(1);
            else return getNpWithinVpAfter(1);
        }
        /// <summary>
        /// this function is used to get parts of sentece as word sequence as a part of a constituent
        /// </summary>
        /// <param name="StartRule"></param>
        /// <param name="EndRule"></param>
        /// <returns></returns>
        public String getParts(int StartRule, int EndRule)
        {
            int startCount = 0;
            for (int i = 0; i < StartRule; i++)
            {
                startCount += countTerm(rules[i], 'a', 'z');
            }
            Console.WriteLine("Startrule  " + StartRule + "  endRule " + EndRule);
            int EndCount = startCount;
            for (int i = StartRule; i <= EndRule; i++)
            {
                EndCount += countTerm(rules[i], 'a', 'z');
            }

            String parts = "";
            for (int i = startCount; i < EndCount; i++)
                parts = parts + " " + words[i];

            return parts.Substring(1);
        }
        /// <summary>
        /// ???
        /// </summary>
        /// <returns></returns>
        public int posOfSen()
        {
            for (int i = 1; i < noOfrules; i++)
                if (rules[i].StartsWith("A"))
                    return i;
            return -1;
        }
        /// <summary>
        /// ???
        /// </summary>
        /// <returns></returns>
        public String getSen()
        {
            int pos = posOfSen();
            if (pos > 0)
            {
                String sen = getParts(0, pos);
                String[] temp = sen.Split(' ');
                sen = "";
                for (int i = 0; i < temp.Length - 1; i++)
                    sen = sen + " " + temp[i];
                if (sen.Length > 1)
                    return sen.Substring(1);
            }
            return null;
        }
    }
}
