import { createContext, useContext } from "react";

const UserContext = createContext(null);

export function UserProvider({ email, children }) {
    return (
        <UserContext.Provider value={email}>
            {children}
        </UserContext.Provider>
    );
}

export function useUser() {
    return useContext(UserContext);
}
