using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class NumberController : MonoBehaviour
{
    // Время движения
    public float finishTime = 1f;

    // Время изменения масштаба
    public float scaleTime = 1f;

    
    // Прошедшее время
    private float elapsedTime;

    // Позиция, куда двигать
    private Vector2 target;

    // Масштаб элемента
    private Vector3 scale = new Vector3(1, 1, 1);

    public void MoveTo(Vector2 target)
    {
        Debug.LogWarning("new target\n" + 
            transform.position.x.ToString("F3") + "||" + transform.position.y.ToString("f3") + '\n' +
            target.x.ToString("F3") + "||" + target.y.ToString("f3"));
        transform.position = this.target;
        this.target = target;
    }

    

	void Start ()
    {
        elapsedTime = 0;
        target = new Vector2(transform.position.x, transform.position.y);
        transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
	}
	
	void Update ()
    {
        elapsedTime += Time.deltaTime;
        transform.position = Vector3.Lerp(transform.position, target, elapsedTime / finishTime);
        if(Math.Abs(transform.position.x - target.x) > 0.001 || Math.Abs(transform.position.y - target.y) > 0.001)
        {
            Debug.LogWarning(transform.position.x.ToString("F3") + "||" + transform.position.y.ToString("f3"));
        }

        transform.localScale = Vector3.Lerp(transform.localScale, scale, elapsedTime / scaleTime);
        //target = Camera.main.ScreenToWorldPoint(new Vector2(Input.mousePosition.x, Input.mousePosition.y));

        //if(Math.Abs(target.x - transform.position.x) > eps ||
        //          Math.Abs(target.y - transform.position.y) > eps)
        //      {
        //          if(Math.Abs(target.x - prevTarget.x) > eps ||
        //              Math.Abs(target.y - prevTarget.y) > eps)
        //          {
        //              transform.position = new Vector3(prevTarget.x, prevTarget.y, 0);
        //              changed = false;
        //          }

        //          if(!changed)
        //          {
        //              changed = true;
        //              dx = (target.x - transform.position.x) / duration;
        //              dy = (target.y - transform.position.y) / duration;
        //          }
        //      }
        //      else
        //      {
        //          if(changed)
        //          {
        //              changed = false;

        //          }
        //      }
    }
}
