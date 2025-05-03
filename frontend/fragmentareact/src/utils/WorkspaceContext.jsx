import { api } from "@/api/fetchClient";
import { createContext, useContext, useState, useEffect } from "react";

const WorkspaceContext = createContext(null);

export function WorkspaceProvider({ role, workspaceId, children }) {
    const [members, setMembers] = useState([]);

    useEffect(() => {
        api.get(`/members`, workspaceId).then(res => setMembers(res));
    }, [workspaceId]);

    function addMembers(newMembers) {
        console.log(newMembers, role, workspaceId)
        api.post(`/members`, { usersId: newMembers}, workspaceId)
            .then(addedMembers => { console.log(members, addedMembers); setMembers([...members, ...addedMembers]) })
    }

    // Remove a member
    function removeMember(memberId) {
        api.delete(`/members/${memberId}`, workspaceId).then(setMembers(members.filter(e => e.id != memberId))).catch(e => console.log(e))
    }

    function grantAdmin(memberId) {
        api.post("/members/" + memberId + "/grant", {}, workspaceId).then(() => setMembers(members.map(e => e.id == memberId ? {...e, role: "Admin"} : e)))
    }

    function revokeAdmin(memberId) {
        api.post("/members/" + memberId + "/revoke", {}, workspaceId).then(() => setMembers(members.map(e => e.id == memberId ? {...e, role: "Member"} : e)))
    }

    // Update member role
    async function updateMemberRole(memberId, newRole) {
        const response = await fetch(`/workspaces/${workspaceId}/members/${memberId}`, {
            method: "PATCH",
            body: JSON.stringify({ role: newRole }),
        });

        if (response.ok) {
            setMembers(prev =>
                prev.map(member =>
                    member.id === memberId ? { ...member, role: newRole } : member
                )
            );
        }
    }
    return (
        <WorkspaceContext.Provider value={{ role, members, addMembers, removeMember, grantAdmin, revokeAdmin}}>
            {children}
        </WorkspaceContext.Provider>
    );
}

export function useWorkspace() {
    return useContext(WorkspaceContext);
}