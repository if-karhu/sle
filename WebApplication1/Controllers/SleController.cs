﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
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
                var parsed = LEParser.parse(parseMe);
                addEquation(parsed);              
            } catch (LEParseException e) {
                ViewBag.Exception = e.Message;
            }
            return View(getSle()); 
        }

        string ok = "<img src=\"/Content/Icons/ok.svg\"/>";
        string no = "<img src=\"/Content/Icons/no.svg\"/>";

        public string parse(string parseme) {
            try {
                LEParser.parse(parseme);
                return ok;
            } catch (LEParseException e) {
                return no;
            }
        }


        public ActionResult Solve() {
            var sle = getSle();
            if (sle.Count == 0) {
                ViewBag.Exception = "No equations";
                return View(INDEX,sle);
            }

            if (sle.Count < sle.First.Value.Item1.Count) {
                ViewBag.Exception = SolverException.NO_OR_INFINITE;
                return View(INDEX, sle);
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
                ViewBag.Exception = SolverException.OVERDEFINED;
                return View(INDEX, sle);
            }

            try {
                var res = GaussianElimination.lsolve(arr, free);             
                ViewBag.Solution = sle.First.Value.Item1.Keys.Zip(res, (name, result) => Tuple.Create(name, result)).ToList();
            } catch (SolverException e) {
                ViewBag.Exception = SolverException.NO_OR_INFINITE;
            }

            return View(INDEX, sle);
        }

        public ActionResult Clear() {
            Session.Clear();
            return View(INDEX, getSle());
        }
    }
}