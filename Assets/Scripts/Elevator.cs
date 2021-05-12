using UnityEngine;
using System.Collections.Generic;
using UnityEditor;

public enum Direction
{
    Stop,
    Up,
    Down
};

public enum Order
{
    Free,
    Up,
    Down
};

public class Elevator : MonoBehaviour
{
    static public float speed = 20.0f;
    public Direction state = Direction.Stop;
    public Order order = Order.Free;
    public int level = 1;
    public int? highLevel = null;
    public int? lowLevel = null;
    public bool[] buttons = new bool[20];
    public float timer = 0.0f;
    public bool[,] taskStop = new bool[2, 20];

    private void Update()
    {
        if (state == Direction.Stop)
        {
            timer -= Time.deltaTime;
            if (buttons[level - 1])
            {
                buttons[level - 1] = false;
                timer = 2.0f;
            }
            if (taskStop[0, level - 1])
            {
                GameManager.Instance.TaskFinished(new UDTask(Direction.Up, level), this);
                taskStop[0, level - 1] = false;
                timer = 2.0f;
            }
            if (taskStop[1, level - 1])
            {
                GameManager.Instance.TaskFinished(new UDTask(Direction.Down, level), this);
                taskStop[1, level - 1] = false;
                timer = 2.0f;
            }
            if (timer <= 0)
            {
                timer = 0;
                if (order == Order.Free)
                {
                    if (highLevel != null && lowLevel == null)
                    {
                        order = Order.Up;
                    }
                    else if (highLevel == null && lowLevel != null)
                    {
                        order = Order.Down;
                    }
                    else if (highLevel != null && lowLevel != null)
                    {
                        if (Mathf.Abs((int)highLevel - level) <= Mathf.Abs(level - (int)lowLevel))
                        {
                            order = Order.Up;
                        }
                        else
                        {
                            order = Order.Down;
                        }
                    }
                }
                state = (Direction)order;
            }
        }
        else
        {
            if (state == Direction.Up)
            {
                transform.position += new Vector3(0, speed * Time.deltaTime);
                int l = GetLevel(true);
                if (l > level)
                {
                    level = l;
                    if (buttons[level - 1])
                    {
                        buttons[level - 1] = false;
                        timer = 2.0f;
                        state = Direction.Stop;
                        if (level == highLevel)
                        {
                            highLevel = null;
                            if (lowLevel == null)
                            {
                                order = Order.Free;
                            }
                            else
                            {
                                order = Order.Down;
                            }
                        }
                        if (taskStop[0, level - 1])
                        {
                            GameManager.Instance.TaskFinished(new UDTask(Direction.Up, level), this);
                            taskStop[0, level - 1] = false;
                        }
                    }
                    else if (taskStop[0, level - 1] || highLevel == level)
                    {
                        GameManager.Instance.TaskFinished(new UDTask(Direction.Up, level), this);
                        timer = 2.0f;
                        state = Direction.Stop;
                        taskStop[0, level - 1] = false;
                        if (level == highLevel)
                        {
                            highLevel = null;
                            if (lowLevel == null)
                            {
                                order = Order.Free;
                            }
                            else
                            {
                                order = Order.Down;
                            }
                        }
                    }
                }
            }
            else if (state == Direction.Down)
            {
                transform.position -= new Vector3(0, speed * Time.deltaTime);
                int l = GetLevel(false);
                if (l < level)
                {
                    level = l;
                    if (buttons[level - 1])
                    {
                        buttons[level - 1] = false;
                        timer = 2.0f;
                        state = Direction.Stop;
                        if (level == lowLevel)
                        {
                            lowLevel = null;
                            if (highLevel == null)
                            {
                                order = Order.Free;
                            }
                            else
                            {
                                order = Order.Up;
                            }
                        }
                        if (taskStop[1, level - 1])
                        {
                            GameManager.Instance.TaskFinished(new UDTask(Direction.Down, level), this);
                            taskStop[1, level - 1] = false;
                        }
                    }
                    else if (taskStop[1, level - 1] || lowLevel == level)
                    {
                        GameManager.Instance.TaskFinished(new UDTask(Direction.Down, level), this);
                        timer = 2.0f;
                        state = Direction.Stop;
                        taskStop[1, level - 1] = false;
                        if (level == lowLevel)
                        {
                            lowLevel = null;
                            if (highLevel == null)
                            {
                                order = Order.Free;
                            }
                            else
                            {
                                order = Order.Up;
                            }
                        }
                    }

                }
            }
        }

    }

    private void Start()
    {
        int i = 0;
        for (; i < 20; i++)
        {
            buttons[i] = false;
            taskStop[0, i] = false;
            taskStop[1, i] = false;

        }
    }

    public int GetLevel(bool isUp)
    {
        int change = 0;
        if (isUp)
        {
            return (int)((transform.position.y - 20) / 50.5 + 1);
        }
        else
        {
            if ((transform.position.y - 20) % 50.5 > 0)
            {
                change = 1;
            }
            if (transform.position.y - 20 <= 0)
            {
                return 1;
            }
            return (int)((transform.position.y - 20) / 50.5 + 1) + change;
        }
    }

    public void GiveUpTask(bool isUp)
    {
        if (isUp)
        {
            for (int i = 0; i < 20; i++)
            {
                if (taskStop[0, i])
                {
                    taskStop[0, i] = false;
                    GameManager.Instance.TaskRestart(new UDTask(Direction.Up, i + 1));
                }
            }
        }
        else
        {
            for (int i = 19; i >= 0; i--)
            {
                if (taskStop[1, i])
                {
                    taskStop[1, i] = false;
                    GameManager.Instance.TaskRestart(new UDTask(Direction.Down, i + 1));
                }
            }
        }

    }
}