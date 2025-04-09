import { api } from '@/api/fetchClient';
import { createContext, useState, useContext, useEffect } from 'react';
import { useParams } from 'react-router';

const BoardContext = createContext(null);

export function BoardProvider({ children }) {
    const { workspaceId, boardId } = useParams()
    const [guests, setGuests] = useState([]);

    console.log("board provider works")

    useEffect(() => {
        api.get(`/boards/${boardId}/guests`, workspaceId).then(res => setGuests(res));
        console.log("fetching guests", guests)
    }, [boardId]);

    function deleteGuest(userId) {
        api.delete(`/boards/${boardId}/guests/${userId}`, workspaceId).then(res => res.status == 204 && setGuests(prev => prev.filter(e => e.id != userId)));
    }

    function addGuests(usersId) {
        api.post(`/boards/${boardId}/guests`, { usersId: usersId }, workspaceId).then(res => setGuests(prev => [...prev, ...res]));
    }

    return (
        <BoardContext.Provider value={{ guests, addGuests, deleteGuest }}>
            {children}
        </BoardContext.Provider>
    );
}

export function useBoard() {
    return useContext(BoardContext);
}