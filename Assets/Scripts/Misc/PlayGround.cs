using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UsefulClasses;
using Debug = UnityEngine.Debug;
using SysDebug = System.Diagnostics.Debug;

public class PlayGround : MonoBehaviour
{
    [SerializeField] private Transform _stickToUITr;
    [SerializeField] private RectTransform _timerTr;

    void Start()
    {
        //var sw = new Stopwatch();

        //// IEnumerable testen:
        //sw.Start();
        //foreach (int number in Filter(GetNumberEnumerable(100), (int number) => number < 20))
        //{
        //    UnityEngine.Debug.Log(number);
        //}
        //sw.Stop();
        //UnityEngine.Debug.Log("IEnumerable: " + sw.ElapsedMilliseconds + "ms");

        //// Array testen:
        //sw.Restart();
        //int[] numbers = GetNumbers(100000);
        //foreach (int number in numbers)
        //{
        //    UnityEngine.Debug.Log(number);

        //}
        //sw.Stop();
        //UnityEngine.Debug.Log("Array: " + sw.ElapsedMilliseconds + "ms");
        DoOnCondition(5, (int number) => number < 4, () => UnityEngine.Debug.Log("test"));
        int[] numbersArray = { 1, 2, 3, 4, 5 };

        var numbers = numbersArray.OrderBy(n => n);
        foreach (var number in numbers)
        {
            // UnityEngine.Debug.Log(number);
        }

        foreach (int number in GetNumberEnumerable(100).Where(n => n < 20))
        {
            //  UnityEngine.Debug.Log($"{number}");
        }
        transform.DeleteChildren();
        Hero[] heroes = { new Hero("Goku", 46), new Hero("Ruffy", 19), new Hero("Triumph", 21), new Hero("Uwa", 99) };
        var newHereos =    heroes.Prepend(new Hero("Delwan", 22));
        IEnumerable<string> allNames = newHereos.Select((hero) => hero._name);
        foreach (var name in allNames)
        {
        }
            
    }
   
    // Update is called once per frame
    void Update()
    {
        _stickToUITr.position = Helpers.GetWorldPositionOfCanvasElement(_timerTr,20);
        if(Mouse.current.leftButton.wasPressedThisFrame && Helpers.GetUIElement() is RectTransform uiElement)
        {
        }
    }

    private IEnumerable<T> Filter<T>(IEnumerable<T> source, Func<T, bool> predicate)
    {
        foreach (var item in source)
        {
            if (predicate(item))
                yield return item;
        }
    }
    private void DoOnCondition<T>(T argument, Func<T, bool> predicate, Action action)
    {
        if (predicate(argument))
        {
            action?.Invoke();
        }
    }
    private IEnumerable<int> GetNumberEnumerable(int length)
    {
        for (int i = 0; i < length; i++)
        {
            yield return i;
        }
    }
    private int[] GetNumbers(int length)
    {
        int[] result = new int[length];
        for (int i = 0; i < length; i++)
            result[i] = i;
        return result;
    }
}
public class Hero 
{
    public string _name;
    public int _age;

    public Hero(string name, int age)
    {
        _name = name;
        _age = age;
    }
}
