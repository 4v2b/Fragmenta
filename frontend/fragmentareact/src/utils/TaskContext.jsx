import { api } from '@/api/fetchClient';
import { createContext, useState, useContext, useEffect } from 'react';
import { useParams } from 'react-router';

const TasksContext = createContext();

export function TasksProvider({ children }) {
  const [tasks, setTasks] = useState([]);
  const { workspaceId, boardId } = useParams()

  function addTask(task, statusId){
    api.post(`/tasks?statusId=${statusId}`, task , workspaceId).then(res => setTasks(res));
  }

  useEffect(() => {
    api.get(`/tasks?boardId=${boardId}`, workspaceId).then(res => setTasks(res));
  }, [boardId]);

  return (
    <TasksContext.Provider value={{ tasks, addTask }}>
      {children}
    </TasksContext.Provider>
  );
}

export function useTasks() {
  return useContext(TasksContext);
}