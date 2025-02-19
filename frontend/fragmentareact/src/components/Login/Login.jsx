import { useState, useEffect } from "react";
import "./Login.css"
import { useTranslation } from 'react-i18next';
import { login } from "../../api"


export function Login() {
    const [ email, setEmail ] = useState("")
    const [ password, setPassword ] = useState("")
    const [ errors, setErrors ] = useState({})

    async function handleLogin() {
        const response = await login(email, password)
        console.log(response)
    }

    const { t } = useTranslation();

    return <>
        <input type="email" onBlur={(e) => setEmail(e.target.value)} placeholder={t("fields.email")}></input>
        <input type="password" onBlur={(e) => setPassword(e.target.value)} placeholder={t("fields.password")} ></input>
        <button onClick={handleLogin} >{t("fields.login")}</button>
    </>
}