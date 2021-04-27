using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ToolsBoxEngine {
    public enum Axis { X, Y, Z }

    #region Nullable vector
    // Nullable Vector
    //public class NVector2 {
    //    public Vector2? vector;

    //    //public static void operator =(NVector2 a, Vector2 b) => a.vector = b;
    //    public static implicit operator Vector2(NVector2 a) => (Vector2)a.vector;
    //    public static implicit operator NVector2(Vector2 a) => new NVector2((Vector2?)a);

    //    public NVector2() {
    //        vector = null;
    //    }

    //    public NVector2(Vector2? vector) {
    //        this.vector = vector;
    //    }

    //    public Vector2 Vector {
    //        get { return (Vector2)vector; }
    //        set { vector = value; }
    //    }

    //    public float x {
    //        get { return Vector.x; }
    //        set { Vector = new Vector2(value, Vector.y); }
    //    }

    //    public float y {
    //        get { return Vector.y; }
    //        set { Vector = new Vector2(Vector.x, value); }
    //    }
    //}
    #endregion

    #region Classes

    public class Nullable<T> where T : struct {
        public T? value;

        public static implicit operator T(Nullable<T> a) => (T)a.value;
        public static implicit operator Nullable<T>(T a) => new Nullable<T>((T?)a);

        public Nullable() {
            value = null;
        }

        public Nullable(T? value) {
            this.value = value;
        }

        public T Value {
            get { return (T)value; }
            set { this.value = value; }
        }
    }

    #endregion

    public static class Tools {

        #region Delegates

        public delegate void BasicDelegate();

        public delegate void BasicDelegate<T>(T arg);

        public delegate T BasicDelegateReturn<T>(T arg);

        public delegate T2 BasicDelegateReturn<T1, T2>(T1 arg);

        #endregion

        #region Extensions methods

        #region Vectors

        public static Vector2 To2D(this Vector3 vector, Axis axisToIgnore = Axis.Z) {
            switch (axisToIgnore) {
                case Axis.X:
                    return new Vector2(vector.y, vector.z);
                case Axis.Y:
                    return new Vector2(vector.x, vector.z);
                case Axis.Z:
                    return new Vector2(vector.x, vector.y);
                default:
                    return new Vector2(vector.x, vector.y);
            }
        }

        public static Vector2Int To2D(this Vector3Int vector, Axis axisToIgnore = Axis.Z) {
            switch (axisToIgnore) {
                case Axis.X:
                    return new Vector2Int(vector.y, vector.z);
                case Axis.Y:
                    return new Vector2Int(vector.x, vector.z);
                case Axis.Z:
                    return new Vector2Int(vector.x, vector.y);
                default:
                    return new Vector2Int(vector.x, vector.y);
            }
        }

        public static Vector3 To3D(this Vector2 vector, float value = 0f, Axis axis = Axis.Z) {
            switch (axis) {
                case Axis.X:
                    return new Vector3(value, vector.x, vector.y);
                case Axis.Y:
                    return new Vector3(vector.x, value, vector.y);
                case Axis.Z:
                    return new Vector3(vector.x, vector.y, value);
                default:
                    return new Vector3(vector.x, vector.y, value);
            }
        }

        public static Vector3Int To3D(this Vector2Int vector, int value = 0, Axis axis = Axis.Z) {
            switch (axis) {
                case Axis.X:
                    return new Vector3Int(value, vector.x, vector.y);
                case Axis.Y:
                    return new Vector3Int(vector.x, value, vector.y);
                case Axis.Z:
                    return new Vector3Int(vector.x, vector.y, value);
                default:
                    return new Vector3Int(vector.x, vector.y, value);
            }
        }

        public static Vector3 Override(this Vector3 vector, float value, Axis axis = Axis.Y) {
            switch (axis) {
                case Axis.X:
                    vector.x = value;
                    break;
                case Axis.Y:
                    vector.y = value;
                    break;
                case Axis.Z:
                    vector.z = value;
                    break;
                default:
                    vector.y = value;
                    break;
            }

            return vector;
        }

        public static Vector2 Override(this Vector2 vector, float value, Axis axis = Axis.Y) {
            switch (axis) {
                case Axis.X:
                    vector.x = value;
                    break;
                case Axis.Y:
                    vector.y = value;
                    break;
                case Axis.Z:
                    Debug.LogWarning("Can't override Vector2 z axis, using default axis : y");
                    vector.y = value;
                    break;
                default:
                    vector.y = value;
                    break;
            }

            return vector;
        }

        public static Vector2Int FloorToInt(this Vector2 vector) {
            return new Vector2Int(Mathf.FloorToInt(vector.x), Mathf.FloorToInt(vector.y));
        }
        
        public static Vector2 Abs(this Vector2 vector) {
            return new Vector2(Mathf.Abs(vector.x), Mathf.Abs(vector.y));
        }

        #endregion

        public static void Print<T>(this Nullable<T>[] array) where T : struct {
            Debug.Log("----------------");
            for(int i = 0; i < array.Length; i++) {
                Debug.Log(i + ". " + array[i].Value);
            }
        }

        public static T[] ToArray<T>(this Nullable<T>[] array) where T : struct {
            T[] returnBack = new T[array.Length];
            for (int i = 0; i < array.Length; i++) {
                returnBack[i] = array[i].Value;
            }
            return returnBack;
        }

        public static Nullable<T>[] ToNullableArray<T>(this T[] array) where T : struct {
            Nullable<T>[] returnBack = new Nullable<T>[array.Length];
            for (int i = 0; i < array.Length; i++) {
                returnBack[i] = (Nullable<T>)array[i];
            }
            return returnBack;
        }

        public static List<T> ToList<T>(this List<Nullable<T>> list) where T : struct {
            List<T> returnBack = new List<T>();
            for (int i = 0; i < list.Count; i++) {
                returnBack.Add(list[i].Value);
            }
            return returnBack;
        }

        public static T2[] Individually<T1, T2>(this T1[] array, BasicDelegateReturn<T1, T2> function) { // Apply a function (with 1 argument) to an Array
            T2[] result = new T2[array.Length];
            for (int i = 0; i < array.Length; i++) {
                result[i] = function(array[i]);
            }
            return result;
        }

        public static float Remap(this float value, float from1, float to1, float from2, float to2) {
            return (value - from1) / (to1 - from1) * (to2 - from2) + from2;
        }

        #endregion
    }
}