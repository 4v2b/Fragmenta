import { createContext, useContext } from "react";

const WorkspaceContext = createContext(null);

export function WorkspaceProvider({ role, children }) {
    return (
        <WorkspaceContext.Provider value={role}>
            {children}
        </WorkspaceContext.Provider>
    );
}

export function useWorkspaceRole() {
    return useContext(WorkspaceContext);
}
