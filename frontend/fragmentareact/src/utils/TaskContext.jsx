import { api } from '@/api/fetchClient';
import { createContext, useState, useContext, useEffect } from 'react';
import { useParams } from 'react-router';

const TasksContext = createContext();

export function TasksProvider({ children }) {
  const [tasks, setTasks] = useState([]);
  const { workspaceId, boardId } = useParams()

  useEffect(() => {
    api.get(`/tasks?boardId=${boardId}`, workspaceId).then(res => setTags(res));
  }, [boardId]);

  return (
    <TasksContext.Provider value={{ tasks}}>
      {children}
    </TasksContext.Provider>
  );
}

export function useTasks() {
  return useContext(TasksContext);
}