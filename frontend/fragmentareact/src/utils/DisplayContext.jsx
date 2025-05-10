import { createContext, useContext, useEffect, useState } from "react";

const DisplayContext = createContext();

const LOCAL_STORAGE_KEY = "taskFieldVisibility";
const defaultFields = ["dueDate", "priority", "tags", "assignee"];

export function DisplayProvider({ children }) {
  const [visibleFields, setVisibleFields] = useState(() => {
    const saved = localStorage.getItem(LOCAL_STORAGE_KEY);
    return saved ? JSON.parse(saved) : defaultFields;
  });

  useEffect(() => {
    localStorage.setItem(LOCAL_STORAGE_KEY, JSON.stringify(visibleFields));
  }, [visibleFields]);

  return (
    <DisplayContext.Provider value={{ visibleFields, setVisibleFields }}>
      {children}
    </DisplayContext.Provider>
  );
}

export function useDisplay() {
  return useContext(DisplayContext);
}
