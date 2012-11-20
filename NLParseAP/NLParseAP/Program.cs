using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NLParseAP
{
    class Program
    {
        public static void println(String header, String str)
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write(header);
            Console.ForegroundColor = ConsoleColor.DarkBlue;
            Console.BackgroundColor = ConsoleColor.White;
            Console.WriteLine(str);
            Console.ResetColor();
        }
        static void Main(string[] args)
        {
            //String str = "he drinks water";
            //String str = "Water is a drink";
            //string str = "sadh born in dhaka";
            //String str = "sadh is in bus";
            //String str = "He played after me";
            //String str = "rice is car";
            //String str = "Chelsea Football Club is an English football club";
            //String str = "Chelses Football Club is located in West London";
            //String str = "Chelses Football Club is not called The Blues";
            //String str = "Chelsea and ManU are club";
            //String str = "Roonie eats rice";
            //String str = "Roonie play football in ManU";
            //string str = "Chelsea football club located in London";
            string str = "I wish happy birthday to you";
            //Console.WriteLine(str.IndexOf('A'));
            PredictiveBacktrackingNLParser nlParser = new PredictiveBacktrackingNLParser();
            PredictiveBacktrackingNLParser obParser = new PredictiveBacktrackingNLParser();
            if (nlParser.parse(str, "A"))
            {
                String VPh = nlParser.getVp(1);
                println("VPh:", VPh);
                //obParser.parse(VPh, "F");
                String sub = nlParser.getSubject();
                println("Sub:", sub);
                String prd = nlParser.getPredicate();
                println("Prd:", prd);
                String obj = nlParser.getObject();
                println("Obj:", obj);
            }
            /*
            println("Sub: ",nlParser.get(1));
            println("Obj: ", nlParser.get(2));
            println("Prd: ", nlParser.getVp(1));
            println("Sub sentence: ",nlParser.getSen());*/
        }
    }
}
