import "./Input.css"

export function Input({ placeholder, type, onInput }) {
    return <>
        <input className="input" type={type} onChange={onInput} placeholder={placeholder}></input>
    </>
}