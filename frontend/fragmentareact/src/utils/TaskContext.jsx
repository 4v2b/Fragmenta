import { api } from '@/api/fetchClient';
import { createContext, useState, useContext, useEffect, useRef } from 'react';
import { useParams } from 'react-router';
import { HubConnectionBuilder } from '@microsoft/signalr';

const BASE_URL = import.meta.env.VITE_SIGNALR_URL;

const TasksContext = createContext();

export function TasksProvider({ children }) {
  const [tasks, setTasks] = useState([]);
  const [types, setTypes] = useState([]);
  const { workspaceId, boardId } = useParams()
  const [allowedExtensions, setAllowedExtensions] = useState([]);
  const [typesId, setTypesId] = useState([])
  const connectionRef = useRef(null);

  console.log(types, allowedExtensions)

  useEffect(() => {
    const names = getLeafTypeName(types, typesId)
    setAllowedExtensions(names);
  }, [typesId, types])

  function getLeafTypeName(nodes, ids) {
    let selectedNames = [];
    nodes.forEach(node => {
      if (node.children?.length > 0) {
        const childNames = getLeafTypeName(node.children, ids);
        selectedNames = [...selectedNames, ...childNames];
      } else if (ids.some(i => i == node.id)) {
        selectedNames.push(node.value);
      }
    });

    return selectedNames;
  }

  useEffect(() => {
    if (!connectionRef.current) {
      const connection = new HubConnectionBuilder()
        .withUrl(`${BASE_URL}/board?boardId=${boardId}`)
        .withAutomaticReconnect()
        .build();

      connection.start()
        .then(() => {
          console.log('Connected to SignalR');
          connection.on('TaskMoved', updatedTask => {
            setTasks(prevTasks =>
              prevTasks.map(task =>
                task.id === updatedTask.id ? { ...task, ...updatedTask } : task
              )
            );
          });
        })
        .catch(err => console.error('SignalR Connection Error: ', err));

      connectionRef.current = connection;
    }
  }, []);

  useEffect(() => {
    api.get(`/attachment-types`, workspaceId).then(res => setTypes(res[0].children));
  }, [])

  function addTask(task, statusId) {
    api.post(`/tasks?statusId=${statusId}`, task, workspaceId).then(res => setTasks([...tasks, res]));
  }

  function shallowUpdateTask(task) {
    api.post(`/tasks/reorder`, { ...task, boardId: boardId }, workspaceId).then(e => {
      setTasks(current =>
        current.map(t =>
          t.id === task.id
            ? { ...t, statusId: task.statusId, weight: task.weight }
            : t
        )
      );
    });
  }

  useEffect(() => {
    api.get(`/tasks?boardId=${boardId}`, workspaceId).then(res => setTasks(res));
  }, [boardId]);

  return (
    <TasksContext.Provider value={{ tasks, setTasks, addTask, allowedExtensions, setTypesId, shallowUpdateTask }}>
      {children}
    </TasksContext.Provider>
  );
}

export function useTasks() {
  return useContext(TasksContext);
}