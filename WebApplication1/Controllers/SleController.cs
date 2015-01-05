using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Sle;
using Sle.Parser;
using Sle.Solver;

namespace WebApplication1.Controllers
{
    public class SleController : Controller
    {
        private const string SLE = "SLE";
        private const string INDEX = "Index";

        private LinkedList<Tuple<SortedDictionary<String, double>, double>> getSle() {
            if (Session.Count == 0) {
                Session.Add(SLE,new LinkedList<Tuple<SortedDictionary<String, double>, double>>());
            }
            return Session[SLE] as LinkedList<Tuple<SortedDictionary<String, double>, double>>;
        }

        private void addEquation(Tuple<SortedDictionary<String, double>, double> newEquation) {
            var sle = getSle();
            if (sle.Count == 0){
                sle.AddLast(newEquation);
                return;
            }            
            var newForAll = newEquation.Item1.Keys.Except(sle.First.Value.Item1.Keys).ToList();
            var allForNew = sle.First.Value.Item1.Keys.Except(newEquation.Item1.Keys).ToList();
            sle.ToList().ForEach(x => newForAll.ForEach(s => x.Item1.Add(s, 0d)));
            allForNew.ForEach(t => newEquation.Item1.Add(t, 0d));           
            sle.AddLast(newEquation);                
        }

        public  ActionResult Index(String parseMe)
        {
            if (String.IsNullOrEmpty(parseMe)) {
                return View(getSle());
            }
            try { 
                var parsed = LEParser.parse(parseMe, ref parseMe);
                addEquation(parsed);              
            } catch (LEParseException e) {
                ViewBag.Exception = e.Message;
            }
            return View(getSle()); 
        }

  
        string ok = "<img src=\"/Content/Icons/ok.png\"/>";
        string no = "<img src=\"/Content/Icons/no.png\"/>";
        string info = "&nbsp;<img class=\"info\" src=\"/Content/Icons/info.png\" />";
        public string parse(string parseme) {
            try {
                LEParser.parse(parseme,ref parseme);
                return ok;
            } catch (LEParseException e) {
                return no + "|"+ info  +"|" + e.Message;
            }
        }

        public string SolveAjax(string[] equations) {
            Clear();
            for (int i = 0; i < equations.Length; i++) {
                addEquation(LEParser.parse(equations[i], ref equations[i]));
            }
            var input = String.Join(String.Empty, equations);
            string output = null;
            List<String> solutions = new List<string>();
            try {
                var res = Solve();                
                foreach (var ans in res){
                    MathMlWriter.init();
                    MathMlWriter.writeStart();
                    MathMlWriter.writeName(ans.Item1);
                    MathMlWriter.writeEquals();
                    if (ans.Item2 < 0) {
                        MathMlWriter.writeSign(false);
                    }
                    MathMlWriter.writeNumber(ans.Item2);
                    MathMlWriter.writeEnd();
                    solutions.Add(MathMlWriter.getMathMl());
                }
                output = MathMlWriter.wrapInTable(String.Join("", solutions));

            } catch (SolverException ex) {
                output = ex.Message;
            }
            return MathMlWriter.wrapInTable(String.Join(String.Empty,equations)) + "|" + output;
        }

        public List<Tuple<string, double>> Solve() {
            var sle = getSle();
            if (sle.Count == 0) {
                throw new SolverException(SolverException.NO_EQUATIONS);
            }

            if (sle.Count < sle.First.Value.Item1.Count) {
                throw new SolverException(SolverException.NO_OR_INFINITE);
            }

            var arr = new double[sle.Count][];
            var free = new double[sle.Count];
            int i = 0;
            foreach (var eq in sle) {
                arr[i] = new double[eq.Item1.Count];
                int j = 0;               
                foreach (var term in eq.Item1){
                    arr[i][j] = term.Value;
                    j++;
                }
                free[i] = eq.Item2;
                i++;
            }

            if (arr[0].Length < free.Length) {
                throw new SolverException(SolverException.OVERDEFINED);
            }

            try {
                var res = GaussianElimination.lsolve(arr, free);             
                return sle.First.Value.Item1.Keys.Zip(res, (name, result) => Tuple.Create(name, result)).ToList();
            } catch (SolverException e) {
                throw e;
            }

          
        }

        public ActionResult Clear() {
            Session.Clear();
            return View(INDEX, getSle());
        }
    }
}