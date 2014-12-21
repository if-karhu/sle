using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Sle.Parser;

namespace WebApplication1.Controllers
{
    public class SleController : Controller
    {
        private const String SLE = "SLE";

        private LinkedList<Tuple<SortedDictionary<String, double>, double>> getSystem() {
            if (Session.Count == 0) {
                Session.Add(SLE,new LinkedList<Tuple<SortedDictionary<String, double>, double>>());
            }
            return Session[SLE] as LinkedList<Tuple<SortedDictionary<String, double>, double>>;
        }


        private void addEquation(Tuple<SortedDictionary<String, double>, double> newEquation) {
            var system = getSystem();
            if (system.Count == 0){
                system.AddLast(newEquation);
                return;
            }
            
            var newForAll = newEquation.Item1.Keys.Except<string>(system.First.Value.Item1.Keys).ToList<String>();
            var allForNew = system.First.Value.Item1.Keys.Except<string>(newEquation.Item1.Keys).ToList<String>();
            system.ToList<Tuple<SortedDictionary<String, double>, double>>().ForEach(x => newForAll.ForEach(s => x.Item1.Add(s, 0d)));
            allForNew.ForEach(t => newEquation.Item1.Add(t, 0d));           
            system.AddLast(newEquation);
                 
        }

        // GET: http://localhost:49168/Sle?parseMe=2n%3D3
        public  ActionResult Index(String parseMe)
        {
            if (String.IsNullOrEmpty(parseMe)) {
                return View(getSystem());
            }
            try { 
                var parsed = LEParser.parse(parseMe);
                addEquation(parsed);
                

            } catch (LEParseException e) {
                ViewBag.Exception = e;
            }

            return View(getSystem()); 
        }
    }
}