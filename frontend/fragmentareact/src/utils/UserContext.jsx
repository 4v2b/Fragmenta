import { api } from "@/api/fetchClient";
import { createContext, useContext, useEffect, useState } from "react";

const UserContext = createContext(null);

export function UserProvider({ children }) {
    const [user, setUser] = useState(null)

    function updateName(name) {
        setUser({ ...user, name });
    }

    useEffect(
        () => {
            api.get("/me").then(res => setUser(res));
        }, [])

    return (
        <UserContext.Provider value={{ userId: user?.id, email: user?.email, userName: user?.name, updateName }}>
            {children}
        </UserContext.Provider>
    );
}

export function useUser() {
    return useContext(UserContext);
}
