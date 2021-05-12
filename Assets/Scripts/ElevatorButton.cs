using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MoveTask
{
    readonly int taskLevel=1;
    public MoveTask(int taskFloor)
    {
        taskLevel = taskFloor;
    }
}

public class ElevatorButton : MonoBehaviour
{
    public static ElevatorButton Instance;
    public Button[] levels;
    public Elevator[] elevators;
    public int elevatorNum = 0;
    public List<Elevator> UpElevators = new List<Elevator>();
    public List<Elevator> DownElevators = new List<Elevator>();
    // Start is called before the first frame update
    void Start()
    {
        int i = 0;
        foreach (Button level in GetComponentsInChildren<Button>())
        {
            levels[i++] = level;
            AddListener(level, i);
        }
    }

    // Update is called once per frame
    void Update()
    {
        for (int i = 0; i < GameManager.Instance.mainTasks.Count;)
        {
            Elevator available = GetAvailavleElevator(i);
            if (available == null)
            {
                i++;
                continue;
            }
            else
            {
                UDTask temp = GameManager.Instance.mainTasks[i];
                AddMoveTask(temp.taskLevel, available);
                if (temp.dir == Direction.Up)
                {
                    available.taskStop[0, temp.taskLevel - 1] = true;
                    UpElevators.Add(available);
                }
                else
                {
                    available.taskStop[1, temp.taskLevel - 1] = true;
                    DownElevators.Add(available);
                }
                GameManager.Instance.MoveTask(i);
            }
        }
        elevatorNum = GameManager.Instance.number.value;
        for (int i = 0; i < levels.Length; i++)
        {
            levels[i].interactable = !elevators[elevatorNum].buttons[i];
        }//按钮显示是否有任务
    }

    private void Awake()
    {
        Instance = this;
        levels = new Button[transform.childCount];
    }
    private void AddListener(Button button, int Index)
    {
        button.onClick.AddListener(delegate ()
        {
            AddMoveTask(Index);
        });
    }
    private void AddMoveTask(int taskLevel)
    {
        Elevator ele = elevators[elevatorNum];
        if (ele.buttons[taskLevel - 1])
        {
            return;
        }
        else
        {
            if (UpElevators.Contains(ele) && taskLevel <= ele.lowLevel)
            {
                UpElevators.Remove(ele);
                ele.GiveUpTask(true);
            }
            else if (DownElevators.Contains(ele) && taskLevel >= ele.highLevel)
            {
                DownElevators.Remove(ele);
                ele.GiveUpTask(false);
            }
            ele.buttons[taskLevel - 1] = true;
            if ((ele.highLevel == null && taskLevel > ele.level) || taskLevel > ele.highLevel)
            {
                ele.highLevel = taskLevel;
            }
            else if ((ele.lowLevel == null && taskLevel < ele.level) || taskLevel < ele.lowLevel)
            {
                ele.lowLevel = taskLevel;
            }
            else if (taskLevel == ele.level)
            {
                if (ele.state == Direction.Up)
                {
                    ele.lowLevel = taskLevel;
                }
                else if (ele.state == Direction.Down)
                {
                    ele.highLevel = taskLevel;
                }
            }
        }
    }

    private void AddMoveTask(int taskLevel, Elevator ele)
    {
        if (ele.buttons[taskLevel - 1])
        {
            return;
        }
        else
        {
            if ((ele.highLevel == null && taskLevel > ele.level) || taskLevel > ele.highLevel)
            {
                ele.highLevel = taskLevel;
            }
            else if ((ele.lowLevel == null && taskLevel < ele.level) || taskLevel < ele.lowLevel)
            {
                ele.lowLevel = taskLevel;
            }
            else if (taskLevel == ele.level)
            {
                if (ele.state == Direction.Up)
                {
                    ele.lowLevel = taskLevel;
                }
                else if (ele.state == Direction.Down)
                {
                    ele.highLevel = taskLevel;
                }
            }
        }
    }

    private Elevator GetAvailavleElevator(int Index)
    {
        UDTask task = GameManager.Instance.mainTasks[Index];
        bool[] sameDir = new bool[elevators.Length];
        int[] distance = new int[elevators.Length];
        for (int i = 0; i < elevators.Length; i++)
        {
            distance[i] = Mathf.Abs(elevators[i].level - task.taskLevel);
            if (elevators[i].order == Order.Free)
            {
                if (elevators[i].timer > 0 && distance[i] != 0)
                {
                    sameDir[i] = false;
                }//电梯没有其他任务但还开着门,且收到的不是当前层的指令时，暂不处理
                else
                {
                    sameDir[i] = true;
                }//未被调度的优先级最高
            }
            else
            {
                if (task.dir == Direction.Up)
                {
                    if (DownElevators.Contains(elevators[i]))
                    {
                        sameDir[i] = false;//已被调度且方向不同不可调度
                    }
                    else if (elevators[i].order == Order.Up)
                    {
                        if (elevators[i].level < task.taskLevel || (elevators[i].level == task.taskLevel && elevators[i].state == Direction.Stop))
                        {
                            sameDir[i] = true;
                        }
                        else
                        {
                            sameDir[i] = false;
                        }
                    }
                    else
                    {
                        sameDir[i] = false;
                    }//未被调度但方向不同
                }
                else
                {
                    if (UpElevators.Contains(elevators[i]))
                    {
                        sameDir[i] = false;//已被调度且方向不同不可调度
                    }
                    else if (elevators[i].order == Order.Down)
                    {
                        if (elevators[i].level > task.taskLevel || (elevators[i].level == task.taskLevel && elevators[i].state == Direction.Stop))
                        {
                            sameDir[i] = true;
                        }
                        else
                        {
                            sameDir[i] = false;
                        }
                    }
                    else
                    {
                        sameDir[i] = false;
                    }
                }
            }//整理其他优先级并更新距离
        }
        int? num = null;
        for (int i = 0; i < elevators.Length; i++)
        {
            if (sameDir[i])
            {
                if (num == null)
                {
                    num = i;
                }
                else if (distance[i] < distance[(int)num])
                {
                    num = i;
                }
            }
        }//选可用的
        if (num != null)
        {
            return elevators[(int)num];
        }
        else
        {
            return null;
        }
    }
}
