import i18n from 'i18next';
import { initReactI18next } from 'react-i18next';

// import Backend from 'i18next-http-backend';
// import LanguageDetector from 'i18next-browser-languagedetector';
// don't want to use this?
// have a look at the Quick start guide 
// for passing in lng and translations on init

i18n
    // .use(Backend)

    // .use(LanguageDetector)
    .use(initReactI18next)
    .init({
        fallbackLng: 'en',
        debug: true,

        interpolation: {
            escapeValue: false, // not needed for react as it escapes by default
        },
        resources: {
            en: {
                translation: {
                    greeting: "Welcome on the main page 🤗",
                    fields:{
                        name: "Name",
                        email: "Email",
                        password : "Password",
                        repeatPassword : "Repeat password",
                        login : "Login"
                    }
                }
            },
            uk: {
                translation: {
                    greeting: "Вітаємо на головній сторінці 🤗",
                    fields:{
                        name: "Ім'я",
                        email: "Ел. пошта",
                        password : "Пароль",
                        repeatPassword : "Повторіть пароль",
                        login : "Увійти"
                    }
                }
            }
        }
    });


export default i18n;