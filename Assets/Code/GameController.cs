using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Game2048;
using System;
using System.IO;
using System.Text;

public class GameController : MonoBehaviour
{
    ////////////////////////////////////////////////////////

    // Блок для рекордов
    public Text textBest;

    // Блок для очков
    public Text textScore;

    // Чувствительность
    public float sensitivity = 3;

    // Префаб плитки
    public GameObject tile;

    // Время движения плиток
    public float moveDuration = 1f;

    // Значение для сравнения
    public float epsilon = 0.2f;

    // Палитра цветов фона
    public Color[] palette;

    // Цвета текста
    public Color[] textPalette;

    // Количество кадров задержки после проигрыша
    public int gameOverDelay = 40;

    // Шаблон диалога конца игры
    public GameObject dialogueTemplate;

    ////////////////////////////////////////////////////////

    // Модель игры
    private Model model;
    
    // Было ли движение => нужно ли ждать отпускание пальца
    private bool moved = false;

    // Сетка плиток
    private GameObject[,] map;

    // Максимум очков
    private int highscore;
    
    // Делегат, вызывающийся после того, как все плитки 
    private Action onTileArrived;

    // Transform элемента, на который помещаются плитки
    private Transform root;

    // Имя сцены настроек
    private readonly string settingsSceneName = "Settings";

    // Переменные для удаления элементов по цепочке
    private int pointerX = 0;
    private int pointerY = 0;
    private int deltaY = 1;

    // Счетчик кадров для задержки после определения проигрыша
    private int missedFrames = 0;

    // Для определения первого кадра после проигрыша
    private bool firstGameOverFrame = true;

    // Для создания единственного окна диалога
    private bool dialogueCreated = false;

    // Можно ли начинать новую игру
    private bool newGameConfirmed = false;

    // Файл настроек
    private string settingsFilePath;
    private readonly string settingsFileName = "/settings.txt";

    // Файл незавершенной сессии
    private string sessionFilePath;
    private readonly String sessionFileName = "/session";

    // Файл рекордов
    private string highscoreFilePath;
    private readonly String highscoreFileName = "/record";

    // Диалог конца игры
    private GameObject dialogue;
    
    ////////////////////////////////////////////////////////

    void Start ()
    {
        settingsFilePath = Application.persistentDataPath + settingsFileName;
        sessionFilePath = Application.persistentDataPath + sessionFileName;
        highscoreFilePath = Application.persistentDataPath + highscoreFileName;



        if (File.Exists(settingsFilePath))
        {
            UploadSettings();
        }
        else
        {
            File.Create(settingsFilePath);
        }
        
        if (File.Exists(highscoreFilePath))
        {
            highscore = ReadHighscore();
            textBest.text = highscore.ToString();
        }
        else
        {
            File.Create(highscoreFilePath);
        }

        int score = -1;
        int[,] data = null;

        if (File.Exists(sessionFilePath))
        {
            score = ReadSessionData(out data);
        }

        onTileArrived = () => { };
        root = GameObject.Find("ROOT").transform;

         
        map = new GameObject[4, 4];
        model = new Model(4);
        model.OnMoved += Model_OnMoved;
        model.OnJoined += Model_OnJoined;
        model.OnItemCreated += Model_OnItemCreated;
        
        model.Start();

        if(data != null && 4 == data.GetLength(0))
        {
            onTileArrived = () => { };
            model.Score = score;

            for (int y = 0; y < model.size; y++)
            {
                for (int x = 0; x < model.size; x++)
                {
                    model.SetNumber(x, y, data[x, y], true);
                }
            }
        }
    }

    private void UploadSettings()
    {
        using (StreamReader reader = new StreamReader(settingsFilePath))
        {
            float sens;
            string rawSens = reader.ReadLine();
            if (float.TryParse(rawSens, out sens))
            {
                sensitivity = sens;
            }
            else
            {
                Debug.LogWarning("Can not parse sensitivity value!\n" + rawSens + " is not looks like a float.");
            }
        }
    }

    private int ReadHighscore()
    {
        using (StreamReader reader = new StreamReader(highscoreFilePath))
        {
            string[] data = reader.ReadToEnd().Split('\n');
            int firstPart;

            if(!Int32.TryParse(data[0], out firstPart))
            {
                return -1;
            }

            int secondPart = 0;
            for(int i = 0; i < data[1].Length; i++)
            {
                int digit = data[1][i];
                secondPart += digit * (int) Math.Pow(10, data[1].Length - i - 1);
            }

            if(firstPart == secondPart)
            {
                return firstPart;
            }
            else
            {
                throw new MissingComponentException(); // Сломает игру
            }
        }
    }

    private void WriteHigscore()
    {
        using (StreamWriter writer = new StreamWriter(highscoreFilePath))
        {
            writer.WriteLine(highscore);
            string score = highscore.ToString();
            char[] buffer = new char[score.Length];
            for(int i = 0; i < score.Length; i++)
            {
                buffer[i] = (char) Int32.Parse(score[i].ToString());
            }
            writer.Write(buffer);
        }
    }

    private void Model_OnJoined(int x, int y, int fx, int fy)
    {
        String name = "b" + fx + "" + fy;

        Vector3 target = GameObject.Find(name).transform.position;

        map[x, y].GetComponent<TileController>().Target = target;
        
        var tile = map[x, y];
        var oldTile = map[fx, fy];

        onTileArrived += () =>
        {
            SetNumber(tile, model.GetMap(fx, fy));
            
            Destroy(oldTile);
            tile.GetComponent<TileController>().RunMergeAnimation();
        };

        map[fx, fy] = map[x, y];
        map[x, y] = null;
    }

    private void Model_OnMoved(int x, int y, int fx, int fy)
    {
        //Debug.Log(x + ":" + y + "|" + fx + ":" + fy);
        String name = "b" + fx + "" + fy;
         
        Vector3 target = GameObject.Find(name).transform.position;
        
        map[x, y].GetComponent<TileController>().Target = target;

        
        map[fx, fy] = map[x, y];
        map[x, y] = null;
        
    }

    private void Model_OnItemCreated(int x, int y)
    {
        if (map[x, y] != null)
        {
            Debug.LogErrorFormat("Creating tile on non-empty place!!!\n{0}:{1}", x, y);
            Destroy(map[x, y]);
        }

        onTileArrived += () =>
        {
            Vector3 position = GameObject.Find("b" + x + "" + y).transform.position;
            //map[x, y] = Instantiate(tile, position, Quaternion.identity, root);
            map[x, y] = Instantiate(tile, root);

            SetNumber(map[x, y], model.GetMap(x, y));
            map[x, y].transform.position = position;
            
            if (Math.Abs(map[x, y].transform.position.x) < 0.001f)
            {
                throw new Exception();
            }
            //Debug.LogFormat("CREATED '{0}' [{1}:{2}]\n{3}\n{4}",model.GetMap(x,y),x,y,position,map[x,y].transform.position);
            
        };
    }

    /// <summary>
    /// Открывает окно настроек
    /// </summary>
    public void OpenSettings()
    {
        SceneManager.LoadScene(settingsSceneName);
    }

    /// <summary>
    /// Закрывает окно настроек
    /// </summary>
    public void CloseSettings()
    {
        SceneManager.UnloadSceneAsync(settingsSceneName);
    }

    void Update()
    {
        // Проверяем конец игры
        if (model.IsGameOver())
        {
            if (missedFrames > gameOverDelay)
            {
                if (!dialogueCreated)
                {
                    onTileArrived();
                    onTileArrived = () => { };
                    dialogue = Instantiate(dialogueTemplate, root);

                    var button = dialogue.GetComponentInChildren<Button>();

                    button.onClick.AddListener(() => newGameConfirmed = true);
                    dialogueCreated = true;
                }

                if (newGameConfirmed)
                {
                    AnotherStepToRestart();
                }

                return;
            }
            else
            {
                missedFrames++;
            }
        }

        bool moving = false;
        for (int x = 0; x < model.size; x++)
        {
            for (int y = 0; y < model.size; y++)
            {
                var element = map[x, y];
                if (element != null)
                {
                    //var target = element.GetComponent<TileController>().Target;
                    //element.transform.position = 
                    //    Vector3.Lerp(element.transform.position, target, 1 / moveDuration);

                    var contr = element.GetComponent<TileController>();

                    if (contr.IsMoving)
                    {
                        moving = true;
                    }

                    if (element.transform.position.x == 0)
                    {

                    }
                }
            }
        }
        if (!moving)
        {
            // Движение закончено, запускаем делегат
            onTileArrived();

            // Присваиваем ему пустую лямбду, чтобы не возиться с проверками на null
            onTileArrived = () => { };
        }


        // Проверка на наличие нажатия 
        if (Input.touchCount > 0)
        {
            // Перемещения клеток еще не было
            if (!moved && Input.GetTouch(0).phase == TouchPhase.Moved)
            {
                // Получаем вектор перемещения пальца
                Vector2 touchDeltaPosition = Input.GetTouch(0).deltaPosition;

                // Вычисляем необходимое изменение на основе чувствительности
                float requiredDelta = 30 / (sensitivity * sensitivity);

                // Если палец недостаточно переместился, выходим
                if (touchDeltaPosition.magnitude < requiredDelta)
                {
                    return;
                }

                Finish();

                // Вызываем делегат, чтобы закончить висящие действия, 
                // поскольку прошлый ход закончен
                onTileArrived();
                onTileArrived = () => { };

                // Поднимаем флагшток
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
                WriteSessionData();
            }
        }
        else
        {
            // Для игры с помощью клавиатуры
            if (Input.anyKeyDown && !moved)
            {
                Finish();
                System.Threading.Thread.Sleep(10);
                moved = true;

                // Вызываем делегат, чтобы закончить висящие действия, 
                // поскольку прошлый ход может быть не закончен
                onTileArrived();
                onTileArrived = () => { };
                

                if (Input.GetKeyDown(KeyCode.UpArrow))
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
                WriteSessionData();
            }
            else
            {
                // Если ничего не произошло, опускаем флагшток
                moved = false;
            }
        }


        // Выводим очки
        int score = model.Score;

        textScore.text = score.ToString();
        if (score > highscore)
        {
            highscore = score;
            textBest.text = score.ToString();
            WriteHigscore();
        }
    }

    private void AnotherStepToRestart()
    {
        if (firstGameOverFrame)
        {
            Destroy(dialogue);
            onTileArrived();
            onTileArrived = () => { };
            firstGameOverFrame = false;
        }
        
        //if (missedFrames < gameOverDelay + 5)
        //{
        //    missedFrames++;
        //    return;
        //}

        if (pointerY >= map.GetLength(0) || pointerY < 0)
        {
            deltaY = -deltaY;
            pointerY += deltaY;

            pointerX++;
            if (pointerX >= map.GetLength(1))
            {
                pointerX = 0;
                pointerY = 0;
                deltaY = 1;
                NewGame();
                
                newGameConfirmed = false;
                dialogueCreated = false;
                firstGameOverFrame = true;
                missedFrames = 0;
                return;
            }
        }
        var element = map[pointerX, pointerY];
        Destroy(element);
        map[pointerX, pointerY] = null;
        pointerY += deltaY;

        //missedFrames = gameOverDelay;
        return;
    }

    int GetColorId(int number)
    {
        var id = Math.Log(number, 2) - 1;

        return (int)id;
    }

    /// <summary>
    /// Завершает всю анимацию
    /// </summary>
    void Finish()
    {
        for (int x = 0; x < model.size; x++)
        {
            for (int y = 0; y < model.size; y++)
            {
                var element = map[x, y];
                if (element != null)
                {
                    var controller = element.GetComponent<TileController>();
                    if (controller.IsMoving)
                    {
                        controller.Finish();
                    }
                }
            }
        }
    }

    /// <summary>
    /// Начинает новую игру
    /// </summary>
    public void NewGame()
    {
        model.Start();
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
        if(number >= 8)
        {
            text.color = textPalette[1];
        }
        else
        {
            text.color = textPalette[0];
        }
        
        int id = GetColorId(number);
        if (id >= 0 && id < palette.Length)
        {
            obj.GetComponent<Image>().color = palette[id];
        }
        else
        {
            Debug.LogError(id);
        }
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

    private void WriteSessionData()
    {
        try
        {
            using (StreamWriter writer = new StreamWriter(sessionFilePath))
            {
                for (int y = 0; y < model.size; y++)
                {
                    StringBuilder builder = new StringBuilder();
                    for (int x = 0; x < model.size; x++)
                    {
                        int mapValue = model.GetMap(x, y);
                        if(mapValue != 0)
                        {
                            int value = (int)Math.Log(mapValue, 2);
                            builder.Append(value);
                        }
                        else
                        {
                            builder.Append(0);
                        }

                        if(x < model.size - 1)
                        {
                            builder.Append(",");
                        }
                    }
                    writer.WriteLine(builder);
                }
                
                writer.Write(model.Score);
            }
        }
        catch (IOException exc)
        {
            Debug.LogError("Can not save game session!\n" + exc.Message);
        }
    }

    private int ReadSessionData(out int[,] map)
    {
        using (StreamReader reader = new StreamReader(sessionFilePath))
        {
            string data = reader.ReadToEnd();
            string[] lines = data.Split('\n');

            map = new int[lines.Length - 1, lines.Length - 1];

            for (int y = 0; y < lines.Length - 1; y++)
            {
                var line = lines[y].Split(',');

                for(int x = 0; x < line.Length; x++)
                {
                    int value = Int32.Parse(line[x]);
                    if(value > 0)
                    {
                        map[x, y] = (int)Math.Pow(2, value);
                    }
                    else
                    {
                        map[x, y] = 0;
                    }
                }
            }

            return Int32.Parse(lines[lines.Length - 1]);
        }
    }
}
