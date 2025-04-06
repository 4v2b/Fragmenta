import { api } from '@/api/fetchClient';
import { createContext, useState, useContext, useEffect } from 'react';
import { useParams } from 'react-router';

const TasksContext = createContext();

export function TasksProvider({ children }) {
  const [tasks, setTasks] = useState([]);
  const { workspaceId, boardId } = useParams()
  const [allowedAttachmentTypes, setAllowedAttachmentTypes] = useState([]);

  function addTask(task, statusId){
    api.post(`/tasks?statusId=${statusId}`, task , workspaceId).then(res => setTasks(res));
  }

  function shallowUpdateTask(task){
    api.post(`/tasks/reorder`, [task], workspaceId);
  }

  useEffect(() => {
    api.get(`/tasks?boardId=${boardId}`, workspaceId).then(res => setTasks(res));
    //api.get(`/allowedTypes?boardId=${boardId}`, workspaceId).then(res => setAllowedAttachmentTypes(res));
  }, [boardId]);

  return (
    <TasksContext.Provider value={{ tasks, setTasks, addTask, allowedAttachmentTypes, shallowUpdateTask }}>
      {children}
    </TasksContext.Provider>
  );
}

export function useTasks() {
  return useContext(TasksContext);
}