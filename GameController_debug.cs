using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Game2048;
using System;

public class GameController : MonoBehaviour
{
    ////////////////////////////////////////////////////////

    // Блок для рекордов
    public Text textBest;

    // Блок для очков
    public Text textScore;

    // Чувствительность
    public float sensitivity = 5;

    // Квадратик с числом
    public GameObject number;
    
    ////////////////////////////////////////////////////////

    // Модель игры
    private Model model;
    
    // Было ли движение => нужно ли ждать отпускание пальца
    private bool moved = false;

    // Сетка объектов
    private GameObject[,] map;

    ////////////////////////////////////////////////////////

    void Start ()
    {
        map = new GameObject[4, 4];
        model = new Model(4);
        model.OnMoved += Model_OnMoved;
        model.OnJoined += Model_OnJoined;
        model.OnItemCreated += Model_OnItemCreated;

        model.Start();
    }

    private void Model_OnJoined(int x, int y, int fx, int fy)
    {
        String name = "b" + fx + "" + fy;

        Vector2 target = GameObject.Find(name).transform.position;
        if(map[x, y] != null)
        {
            map[x, y].GetComponent<NumberController>().MoveTo(target);
        }
        else
        {
            Debug.Log("JOIINNNNNNNNN " + x + " " + y);
            ShowDebug();
            return;
        }

        SetNumber(map[x, y], model.GetMap(fx, fy));
        Destroy(map[fx, fy]);

        map[fx, fy] = map[x, y];
        map[x, y] = null;

        if(!Check())
        {
            Debug.Log("JOIN_ERROR");
            Check();
        }
    }

    private void Model_OnMoved(int x, int y, int fx, int fy)
    {
        String name = "b" + fx + "" + fy;

        Vector2 target = GameObject.Find(name).transform.position;
        
        if(map[x, y] != null)
        {
            map[x, y].GetComponent<NumberController>().MoveTo(target);
        }
        else
        {
            Debug.Log("MOOOOOVEEEEEEE " + x + " " + y);
            ShowDebug();
            return;
        }

        if(map[fx, fy] != null)
        {
            Debug.Log("[from " + x + ":" + y + "]\n" + fx + ":" + fy + " is not empty :/");
        }
        map[fx, fy] = map[x, y];
        map[x, y] = null;

        if (!Check())
        {
            Debug.Log("MOVE_ERROR");
            Check();
        }
    }

    private void Model_OnItemCreated(int x, int y)
    {
        Debug.Log(x + " " + y);
        if (map[x, y] != null)
        {
            Destroy(map[x, y]);
        }
        Vector3 position = GameObject.Find("b" + x + "" + y).transform.position;
        Transform root = GameObject.Find("ROOT").transform;
        map[x, y] = Instantiate(number, position, Quaternion.identity, root);

        SetNumber(map[x, y], model.GetMap(x, y));
    }

    void Update ()
    {
        // Проверка на наличие нажатия 
        if (Input.touchCount > 0)
        {
            // Перемещения клеток еще не было
            if (!moved && Input.GetTouch(0).phase == TouchPhase.Moved)
            {
                // Получаем вектор перемещения пальца
                Vector2 touchDeltaPosition = Input.GetTouch(0).deltaPosition;

                // Если палец недостаточно переместился, выходим
                if(touchDeltaPosition.magnitude < sensitivity)
                {
                    return;
                }
                
                moved = true;

                // Получаем изменения координат
                float sx = touchDeltaPosition.x;
                float sy = touchDeltaPosition.y;

                // Вызываем нужный метод в зависимости от направления
                if (Math.Abs(sx) > Math.Abs(sy))
                {
                    if (sx < 0)
                    {
                        model.Left();
                    }
                    else
                    {
                        model.Right();
                    }
                }
                else
                {
                    if (sy > 0)
                    {
                        model.Up();
                    }
                    else
                    {
                        model.Down();
                    }
                }

                // Проверяем конец игры
                if (model.IsGameOver())
                {
                    // ЧОТА СДЕЛАТЬ
                }

                // Показываем всю картину
                Show();
            }
        }
        else
        {
            moved = false;
        }

        if (Input.anyKeyDown && !moved)
        {
            moved = true;
            if(Input.GetKeyDown(KeyCode.UpArrow))
            {
                model.Up();
            }
            else
            {
                if (Input.GetKeyDown(KeyCode.DownArrow))
                {
                    model.Down();
                }
                else
                {
                    if (Input.GetKeyDown(KeyCode.LeftArrow))
                    {
                        model.Left();
                    }
                    else
                    {
                        if (Input.GetKeyDown(KeyCode.RightArrow))
                        {
                            model.Right();
                        }
                        else
                        {
                            moved = false;
                        }
                    }
                }
            }
            ShowDebug();
        }
        else
        {
            moved = false;
        }

        // Выводим очки
        textScore.text = model.Score.ToString();
    }

    public void ShowDebug()
    {
        String Umap = "";
        String Mmap = "";
        for (int y = 0; y < model.size; y++)
        {
            for (int x = 0; x < model.size; x++)
            {
                if (map[x, y] != null)
                {
                    var text = map[x, y].GetComponentInChildren<Text>();
                    Umap += text.text + " ";
                }
                else
                {
                    Umap += "0 ";
                }
                Mmap += model.GetMap(x, y) + " ";
            }
            Umap += " U\n";
            Mmap += " M\n";
        }
        Debug.Log(Umap);
        Debug.Log(Mmap);
    }

    /// <summary>
    /// Выводит значения на поле
    /// </summary>
    void Show()
    {
        for (int x = 0; x < model.size; x++)
        {
            for (int y = 0; y < model.size; y++)
            {
                ShowButtonText("b" + x + y, model.GetMap(x, y));
            }
        }
    }

    /// <summary>
    /// Записывает значение в определенную клетку
    /// </summary>
    /// <param name="name">Имя клетки</param>
    /// <param name="number">Числовое значение</param>
    private void ShowButtonText(string name, int number)
    {
        var button = GameObject.Find(name);
        var text = button.GetComponentInChildren<Text>();
        if(number != 0)
        {
            text.text = number.ToString();
        }
        else
        {
            text.text = " ";
        }
    }

    /// <summary>
    /// Начинает новую игру
    /// </summary>
    public void NewGame()
    {
        model.Start();
        Show();
    }

    /// <summary>
    /// Устанавливает нужному объекту числовое значение
    /// </summary>
    /// <param name="obj">Объект, движущийся элемент игры</param>
    /// <param name="number">Число</param>
    public void SetNumber(GameObject obj, int number)
    {
        var text = obj.GetComponentInChildren<Text>();
        text.text = number.ToString();
    }

    public void RandomTurn()
    {
        float value = UnityEngine.Random.value;
        if(value > 0.5f)
        {
            if(value > 0.75f)
            {
                Debug.Log("UP");
                model.Up();
                
            }
            else
            {
                Debug.Log("RIGHT");
                model.Right();
                
            }
        }
        else
        {
            if (value < 0.25f)
            {
                Debug.Log("DOWN");
                model.Down();
                
            }
            else
            {
                Debug.Log("LEFT");
                model.Left();
                
            }
        }
    }

    public bool Check()
    {
        for (int x = 0; x < model.size; x++)
        {
            for (int y = 0; y < model.size; y++)
            {
                if(null == map[x, y])
                {
                    continue;
                }
                var text = map[x, y].GetComponentInChildren<Text>();
                int number = Int32.Parse(text.text);
                if (number != model.GetMap(x, y))
                {
                    return false;
                }
            }
        }
        return true;
    }
}
