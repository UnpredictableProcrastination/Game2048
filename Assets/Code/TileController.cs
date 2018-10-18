using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class TileController : MonoBehaviour
{
    // Время изменения масштаба
    public float scaleTime = 0.7f;

    // Максимальное увеличение плитки при слиянии
    public float maxMergeScale = 1.1f;

    // Позиция, куда двигать плитку
    private Vector2 target;
    public Vector2 Target
    {
        get
        {
            return target;
        }
        set
        {
            target = value;
        }
    }

    // Прошедшее время
    private float elapsedTime = 0;

    // Масштаб плитки, к которому она стремится
    private Vector3 scale = new Vector3(1, 1, 1);

	void Start ()
    {
        target = new Vector2(transform.position.x, transform.position.y);
        transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
	}
	
	void Update ()
    {
        elapsedTime += Time.deltaTime;

        transform.localScale = Vector3.Lerp(transform.localScale, scale, elapsedTime / scaleTime);

        if (elapsedTime > scaleTime / 2)
        {
            // Прошло достаточно времени, возвращаем исходные размеры
            scale = new Vector3(1, 1, 1);
        }
    }

    public void RunMergeAnimation()
    {
        scale = new Vector3(maxMergeScale, maxMergeScale, maxMergeScale);
        elapsedTime = 0;
    }

    public void Finish()
    {
        transform.position = target;
        scale = new Vector3(1, 1, 1);
        transform.localScale = scale;
    }
}
