export function canAddMember(role) {
    return role === "Owner" || role === "Admin";
}

export function canEditBoard(role) {
    return role === "Owner" || role === "Admin";
}

export function canManageBoardContent(role) {
    return role !== "Guest";
}

export function canCreateBoard(role) {
    return role === "Owner" || role === "Admin";
}

export function canEditWorkspace(role) {
    return role === "Owner";
}

export function canDeleteMembers(role) {
    return role === "Owner" || role === "Admin";
}

export function canDeleteMember(actorRole, memberRole) {
    return ((memberRole != "Owner" && memberRole != "Guest" ) && actorRole == "Owner") ||
        (actorRole == "Admin" && (memberRole == "Member"))
}

export function canGrantAdmin(actorRole, userRole){
    return actorRole == "Owner" && userRole == "Member";
}

export function canRevokeAdmin(actorRole, userRole){
    return actorRole == "Owner" && userRole == "Admin";
}

export function canLeaveWorkspace(role) {
    return role === "Admin" || role === "Member";
}