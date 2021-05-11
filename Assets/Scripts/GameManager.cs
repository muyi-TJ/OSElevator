using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UDTask
{
    public readonly Direction dir;
    public readonly int taskLevel = 1;
    public UDTask(Direction direction,int taskFloor)
    {
        dir = direction;
        taskLevel = taskFloor;
    }
}


public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public Dropdown floor;
    public Dropdown number;
    public Button upButton;
    public Button downButton;
    public List<UDTask> mainTasks = new List<UDTask>();
    public List<UDTask> dispachedTasks = new List<UDTask>();
    public bool[,] buttonInteractable = new bool[2, 20];
    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < 20; i++)
        {
            buttonInteractable[0, i] = true;
            buttonInteractable[1, i] = true;
        }
        buttonInteractable[0, 19] = false;
        buttonInteractable[1, 0] = false;
        upButton.onClick.AddListener(delegate ()
        {
            AddUDTask(Direction.Up);
            buttonInteractable[0, floor.value] = false;
        });
        downButton.onClick.AddListener(delegate ()
        {
            AddUDTask(Direction.Down);
            buttonInteractable[1, floor.value] = false;
        });
    }

    // Update is called once per frame
    void Update()
    {
        upButton.interactable = buttonInteractable[0, floor.value];
        downButton.interactable = buttonInteractable[1, floor.value];
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }
    }

    private void Awake()
    {
        Instance = this;
    }

    private void AddUDTask(Direction direction)
    {
        int tasklevel = floor.value + 1;
        mainTasks.Add(new UDTask(direction, tasklevel));
    }//此处添加外部调度任务

    public void MoveTask(int Index)
    {
        dispachedTasks.Add(mainTasks[Index]);
        mainTasks.RemoveAt(Index);
    }

    public void TaskFinished(UDTask task, Elevator elevator)
    {
        Debug.Log(elevator);
        dispachedTasks.Remove(task);
        Debug.Log(task.taskLevel);
        if (task.dir == Direction.Up)
        {
            buttonInteractable[0, task.taskLevel - 1] = true;
            if (elevator.taskStop.Equals(new bool[2, 20]))
            {
                ElevatorButton.Instance.UpElevators.Remove(elevator);
            }
        }
        else
        {
            buttonInteractable[1, task.taskLevel - 1] = true;
            if (elevator.taskStop.Equals(new bool[2, 20]))
            {
                ElevatorButton.Instance.DownElevators.Remove(elevator);
            }
        }
    }
};
