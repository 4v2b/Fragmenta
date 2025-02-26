import { useTranslation } from "react-i18next";
import { Button } from "../Button/Button";
import "./ListItem.css"

export function ListItem({ data, onDelete = null }) {

    const {t} = useTranslation()

    
    return <div className="item">
        <div className="section">
            {data.name}
        </div>
        <div className="section">
            {data.email}
        </div>
        <div className="section">
            { }
        </div>
        {onDelete && <Button content={t("fields.delete")} onClick={() =>onDelete(data.id)} /> } 
    </div>
}