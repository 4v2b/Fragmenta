export function storeToken(name, value){
    localStorage.setItem(name, value)
}

export function retrieveToken(name){
    localStorage.getItem(name)
}