import "./Button.css"

export function Button({ content, onClick }) {
    return <div className="button" onClick={onClick}>
        {content}
    </div>
}