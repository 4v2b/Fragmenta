export function canAddMember(role) {
    return role === "Owner" || role === "Admin";
}

export function canEditBoard(role) {
    return role === "Owner" || role === "Admin";
}

export function canEditWorkspace(role) {
    return role === "Owner";
}

export function canDeleteMembers(role) {
    return role === "Owner" || role === "Admin";
}

export function canDeleteMember(actorRole, memberRole) {
    return (memberRole != "Owner" && actorRole == "Owner") ||
        (actorRole == "Admin" && (memberRole == "Member" || memberRole == "Guest"))
}