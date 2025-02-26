
import { Link } from "react-router"
import { register } from "../../api/api"
import { Button } from "../../components/Button/Button"
import { useState } from "react"
import { useTranslation } from "react-i18next"
import "./Register.css"

export function Register() {
    const [email, setEmail] = useState("")
    const [password, setPassword] = useState("")
    const [passwordCopy, setPasswordCopy] = useState("")

    const [name, setName] = useState("")

    const [errors, setErrors] = useState({})

    async function handleRegister() {
        if (password == passwordCopy) {
            const response = await register(name, email, password)
            console.log(response)
        }
    }

    const { t } = useTranslation();

    return <div className="register-content">
        <input type="email" onBlur={(e) => setEmail(e.target.value)} placeholder={t("fields.email")}></input>
        <input onBlur={(e) => setName(e.target.value)} placeholder={t("fields.name")}></input>

        <input type="password" onBlur={(e) => setPassword(e.target.value)} placeholder={t("fields.password")} ></input>
        <input type="password" onBlur={(e) => setPasswordCopy(e.target.value)} placeholder={t("fields.repeatPassword")} ></input>
        <Link to="/login" >{t("auth.toLogin")}</Link>
        <Button onClick={handleRegister} content={t("fields.register")} />
    </div>
}