using System;
using System.Collections;
using System.Collections.Generic;

namespace NeuralNet
{
    public class NN
    {
        string _name = "Neural Network";
        public string name { get { return "Forrest " + _name; } }
        float[] _hL;
        public int hL { get { return _hL.Length; } }
        int _inputs;
        public int inputs { get { return _inputs; } }
        float[][] _hLw;
        public int weightCount { get { return TotalWeightCount(); } }
        float[] _hLb;
        float[] Ow;
        float Ob;
        float _fitness;
        public double fitness { get { return _fitness; } }

        /// <summary>
        /// Sometimes you just want access to variables like fitness & don't need to pass in node counts
        /// </summary>
        public NN()
        {

        }

        /// <summary>
        /// In most cases its important to assign input & Hidden Layer node count
        /// </summary>
        /// <param name="inpsNumb"></param>
        /// <param name="_hLNodes"></param>
        public NN(int inpsNumb, int _hLNodes)
        {
            _hL = new float[_hLNodes];
            IniWeights(inpsNumb);
            _inputs = inpsNumb;
        }

        /// <summary>
        /// Add a fitness value to this network
        /// </summary>
        /// <param name="fit"></param>
        public void SetFitness(float fit)
        {
            _fitness = fit;
        }

        /// <summary>
        /// Inputing an interager (number of _inputs) will initialize all weights with zeros
        /// </summary>
        /// <param name="inps">number of _inputs</param>
        void IniWeights(int inps)
        {
            // 
            _inputs = inps;

            // 
            Ow = new float[_hL.Length];

            // Set a double nested array, [hidden layer length][input length]
            _hLw = new float[_hL.Length][];

            // Set the array for biases that are = to the length of nodes
            _hLb = new float[_hLw.Length];

            // Next we loop through each input array nest & set the length of each to the number of _inputs
            for (int i = 0; i < _hLw.GetLength(0); i++)
            {
                _hLw[i] = new float[inps];
                _hLb[i] = 0;// r.Next(-cap, cap);
                Ow[i] = 0;// r.Next(-cap, cap);

                // 
                for (int j = 0; j < _hLw[i].Length; j++)
                {
                    _hLw[i][j] = 0;// r.Next(-cap, cap);
                }
            }
        }

        /// <summary>
        /// This function is used for loading weight matricies (brains/DNA/whatever you want to call it) Inputting a float array will set the weights to this array
        /// </summary>
        /// <param name="w"></param>
        public void IniWeights(float[] w)
        {
            // Okay don't freak out from this code let's walk through it step by step.

            // first were going to run a for loop the length of the number of nodes in our Hidden Layer
            for (int i = 0; i < _hLw.Length; i++)
            {
                // then we're going to map all of the hidden layer weights from our float vector input w
                // & because I encode the input vector w as all _hL[0] weights then _hL[0] bias, then _hL[1] weight -> _hL[1] bias
                // all the way to it's length, then do all Output weights, then Output bias, I am going to map my Neural Net accordingly 
                for (int j = 0; j < _hLw[i].Length; j++)
                {
                    // this is exactly what I described above but in math form
                    _hLw[i][j] = w[j + (i * 6)];
                }

                // then we're going to map all the biases for ALL nodes in the Hidden Layer, as described above but in math form
                _hLb[i] = w[5 + (i * 6)];

                // Then we're going to map all of the Output weights, as described above but in math form
                Ow[i] = w[i + 24];

                // & lastly the Output bias is the last on the input w sequence & we're done!
                Ob = w[w.Length - 1];
            }
        }

        /// <summary>
        /// This function is used for loading weight matricies (brains/DNA/whatever you want to call it) Inputting a string with values separeted with ","s will convert the string into a float array & use that as weights
        /// </summary>
        /// <param name="w"></param>
        public void IniWeights(string inp)
        {
            float[] w = new float[weightCount];

            for (int i = 0; i < w.Length; i++)
            {
                w[i] = float.Parse(inp.Split(',')[i]);
            }

            // Okay don't freak out from this code let's walk through it step by step.

            // first were going to run a for loop the length of the number of nodes in our Hidden Layer
            for (int i = 0; i < _hLw.Length; i++)
            {
                // then we're going to map all of the hidden layer weights from our float vector input w
                // & because I encode the input vector w as all _hL[0] weights then _hL[0] bias, then _hL[1] weight -> _hL[1] bias
                // all the way to it's length, then do all Output weights, then Output bias, I am going to map my Neural Net accordingly 
                for (int j = 0; j < _hLw[i].Length; j++)
                {
                    // this is exactly what I described above but in math form
                    _hLw[i][j] = w[j + (i * 6)];
                }

                // then we're going to map all the biases for ALL nodes in the Hidden Layer, as described above but in math form
                _hLb[i] = w[5 + (i * 6)];

                // Then we're going to map all of the Output weights, as described above but in math form
                Ow[i] = w[i + 24];

                // & lastly the Output bias is the last on the input w sequence & we're done!
                Ob = w[w.Length - 1];
            }
        }

        /// <summary>
        /// This will return the output aka guess for the joystick movement
        /// </summary>
        /// <param name="inps"></param>
        /// <returns></returns>
        public float CalculateNN(float[] inps)
        {
            // Set the value of all hidden layer outputs
            for (int i = 0; i < _hL.Length; i++)
            {
                _hL[i] = ReLU(Sum(inps, _hLw[i]) + _hLb[i]);
            }

            float O = SoftSign(Sum(Ow, _hL) + Ob);
            //float O = ACTIVATION(Sum(Ow, _hL) + Ob);

            return O;
        }

        /// <summary>
        /// The summation of a[0-n] * b[0-n]
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        float Sum(float[] a, float[] b)
        {
            float ret = 0;
            for (int i = 0; i < a.Length; i++)
            {
                ret += a[i] * b[i];
            }
            return ret;
        }

        /// <summary>
        /// ReLU activation function returns the max between 0 & your inp
        /// </summary>
        /// <param name="inp"></param>
        /// <returns></returns>
        float ReLU(float inp)
        {
            return Math.Max(0, inp);
        }

        /// <summary>
        /// Softsign is an activation function that maps your input in between -1 & 1
        /// </summary>
        float SoftSign(float inp)
        {
            return inp / (1 + Math.Abs(inp));
        }

        /// <summary>
        /// This returns the Genetic Code Sequence for the attempt in an easy readable string format
        /// </summary>
        /// <returns></returns>
        public string ReadBrain()
        {
            string dna = "";

            for (int i = 0; i < _hLw.Length; i++)
            {
                for (int j = 0; j < _hLw[i].Length; j++)
                {
                    dna += _hLw[i][j] + ",";
                }
                dna += _hLb[i] + ",";
            }

            for (int i = 0; i < Ow.Length; i++)
            {
                dna += Ow[i] + ",";
            }

            dna += Ob;

            return dna;
        }

        /// <summary>
        /// This returns the Genetic Code Sequence for the attempt in a float array
        /// </summary>
        /// <returns></returns>
        public float[] GetBrain()
        {
            string[] dna = ReadBrain().Split(',');

            float[] ret = new float[dna.Length];

            for (int i = 0; i < dna.Length; i++)
            {
                ret[i] = float.Parse(dna[i]);
            }

            return ret;
        }

        /// <summary>
        /// Returns the total weight count
        /// </summary>
        /// <returns></returns>
        int TotalWeightCount()
        {
            return (_hLw[0].Length * _inputs) + Ow.Length;
        }

        /// <summary>
        /// Sets the name of the NN
        /// </summary>
        /// <param name="n"></param>
        public void SetName(string n)
        {
        _name = n;
        }
    }
}
