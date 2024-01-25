
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;


public class Gest {
        public readonly static double MIN_DIST = 0.03;

        private ArrayList pos_sequence;

        public Gest() {
            this.pos_sequence = new ArrayList();
        }

        public void addPos(Vector2 pos) {
            double min_dist = double.PositiveInfinity;
            foreach (Vector2 tp in pos_sequence) {
                double d = Vector2.Distance(pos, tp);
                if (d < min_dist) {
                    min_dist = d;
                }
            }
            if (min_dist >= MIN_DIST) {
                this.pos_sequence.Add(pos);
            }
        }

        public void clear() {
            this.pos_sequence = new ArrayList();
        }

        public bool isEmpty() {
            return this.pos_sequence.Count == 0;
        }

        /* Given another gesture, compute how far off they are
        */
        public double getAccuracy(Gest otherGest) {
            List<double> accuracies = new List<double>();

            // Loop positions
            foreach (Vector2 tp in pos_sequence) {
                // Find closest distance to any point in other gesture and add that as local accuracy
                double local_accuracy = double.PositiveInfinity;
                foreach (Vector2 tpother in otherGest.pos_sequence) {
                    double d = Vector2.Distance(tp, tpother);
                    if (d < local_accuracy) {
                        local_accuracy = d;
                    }
                }
                accuracies.Add(local_accuracy);
            }

            double acc = accuracies.Average() * (Math.Abs(pos_sequence.Count - otherGest.pos_sequence.Count) + 1);
            return acc;
        }

        public void printPositions() {
            string s = "";
            foreach (Vector2 tp in pos_sequence) {
                s += "GESTURE.addPos(new Vector2((float)" + tp.x + ", (float)" + tp.y + "));\n";
            }
            StreamWriter fp = new StreamWriter("./gesture.txt");
            fp.WriteLine(s);
            fp.Close();
        }
    }


class Const {
    public static Gest GESTURE = new Gest();
    
    static Const() {
        GESTURE.addPos(new Vector2((float)0.1773288, (float)0.5218341));
        GESTURE.addPos(new Vector2((float)0.1874299, (float)0.4912664));
        GESTURE.addPos(new Vector2((float)0.1997755, (float)0.4628821));
        GESTURE.addPos(new Vector2((float)0.2132435, (float)0.4323144));
        GESTURE.addPos(new Vector2((float)0.2300786, (float)0.3951965));
        GESTURE.addPos(new Vector2((float)0.2424242, (float)0.3668122));
        GESTURE.addPos(new Vector2((float)0.2592593, (float)0.3406114));
        GESTURE.addPos(new Vector2((float)0.2839506, (float)0.3209607));
        GESTURE.addPos(new Vector2((float)0.312009, (float)0.3056768));
        GESTURE.addPos(new Vector2((float)0.342312, (float)0.3122271));
        GESTURE.addPos(new Vector2((float)0.3647587, (float)0.3340611));
        GESTURE.addPos(new Vector2((float)0.3838384, (float)0.3624454));
        GESTURE.addPos(new Vector2((float)0.3995511, (float)0.3886463));
        GESTURE.addPos(new Vector2((float)0.4118967, (float)0.4170306));
        GESTURE.addPos(new Vector2((float)0.4264871, (float)0.4475982));
        GESTURE.addPos(new Vector2((float)0.4377104, (float)0.4759825));
        GESTURE.addPos(new Vector2((float)0.4478115, (float)0.5043668));
        GESTURE.addPos(new Vector2((float)0.4713805, (float)0.5262009));
        GESTURE.addPos(new Vector2((float)0.5028058, (float)0.5283843));
        GESTURE.addPos(new Vector2((float)0.5297419, (float)0.510917));
        GESTURE.addPos(new Vector2((float)0.5544332, (float)0.4934498));
        GESTURE.addPos(new Vector2((float)0.5757576, (float)0.4628821));
        GESTURE.addPos(new Vector2((float)0.5959596, (float)0.4344978));
        GESTURE.addPos(new Vector2((float)0.6206509, (float)0.4061135));
    }
}
