using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class TileController : MonoBehaviour
{
    // Время изменения масштаба
    public float scaleTime = 0.1f;

    // Время движения плитки
    public float moveTime = 3f;

    // Максимальное увеличение плитки при слиянии
    public float maxMergeScale = 1.1f;

    // Позиция, куда двигать плитку
    private Vector3 target;
    public Vector3 Target
    {
        get
        {
            return target;
        }
        set
        {
            if(transform.position.x == 0)
            {
                //throw new Exception();
            }
            transform.position = new Vector3(target.x, target.y, target.z);
            target = value;
        }
    }

    /// <summary>
    /// Двигается ли плитка к цели в данный момент
    /// </summary>
    public bool IsMoving
    {
        get
        {
            return Math.Abs(transform.position.x - target.x) > epsilon
                || Math.Abs(transform.position.y - target.y) > epsilon;
        }
    }

    // Значение для сравнения
    private float epsilon = 0.2f;

    // Прошедшее время
    private float elapsedTime = 0;

    // Масштаб плитки, к которому она стремится
    private Vector3 scale = new Vector3(1, 1, 1);

	void Start ()
    {
        if (transform.position.x == 0)
        {
            //throw new NotSupportedException();
        }
        if (Math.Abs(transform.position.x) > 0.001)
        {
            target = new Vector3(transform.position.x, transform.position.y, transform.position.z);
        }
        else
        {
            transform.position = target;
        }
        transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
    }
    
	void Update ()
    {
        if(transform.position.x == 0)
        {
            //throw new NotSupportedException();
        }

        elapsedTime += Time.deltaTime;

        //transform.position = Vector3.Lerp(transform.position, target, 1 / moveTime);
        Vector3 move = target - transform.position;
        transform.position += move / moveTime;

        transform.localScale = Vector3.Lerp(transform.localScale, scale, elapsedTime / scaleTime);

        if (elapsedTime > scaleTime / 2)
        {
            // Прошло достаточно времени, возвращаем исходные размеры
            scale = new Vector3(1, 1, 1);
        }
    }

    /// <summary>
    /// Запуск анимации слияния
    /// </summary>
    public void RunMergeAnimation()
    {
        scale = new Vector3(maxMergeScale, maxMergeScale, maxMergeScale);
        elapsedTime = 0;
    }

    /// <summary>
    /// Завершает все анимации (движение и масштабирование)
    /// </summary>
    public void Finish()
    {
        transform.position = target;
        scale = new Vector3(1, 1, 1);
        transform.localScale = scale;
    }
}
