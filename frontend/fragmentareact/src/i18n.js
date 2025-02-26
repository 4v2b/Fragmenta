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
                    roles : {
                        admin: "Admin",
                        owner : "Owner",
                        member: "Member",
                        guest: "Guest"
                    },
                    greeting: "Welcome on the main page 🤗",
                    fields:{
                        name: "Name",
                        email: "Email",
                        password : "Password",
                        repeatPassword : "Repeat password",
                        login : "Login",
                        register : "Register",
                        logout: "Log out",
                        delete : "Delete",
                        addMembers : "Add members",
                        findUser : "Find user"
                    },
                    auth:{
                        toLogin: "Already have an account?",
                        toRegister: "Do not have any account?"
                    },
                    createWorkspace: "Create workspace",
                    workspaceStub: "None",
                    boards: "Boards",
                    members : "Members",
                    general : "General"
                }
            },
            uk: {

                translation: {
                    roles : {
                        admin: "Адмін",
                        owner : "Власник",
                        member: "Учасник",
                        guest: "Гість"
                    },
                    greeting: "Вітаємо на головній сторінці 🤗",
                    fields:{
                        name: "Ім'я",
                        email: "Ел. пошта",
                        password : "Пароль",
                        repeatPassword : "Повторіть пароль",
                        login : "Увійти",
                        register : "Зареєструватися",
                        logout: "Вийти",
                        delete : "Видалити",
                        
                        addMembers : "Додати учасників",
                        findUser : "Знайти користувача"
                    },
                    auth:{
                        toLogin: "Вже маєте обліковий запис?",
                        toRegister: "Не маєте облікового запису?"
                    },

                    createWorkspace: "Новий робочий простір",
                    workspaceStub: "Не обрано",
                    boards: "Дошки",
                    members : "Учасники",
                    general : "Загальне"
                }
            }
        }
    });


export default i18n;