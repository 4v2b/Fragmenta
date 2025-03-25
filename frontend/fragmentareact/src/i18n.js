import i18n from 'i18next';
import { initReactI18next } from 'react-i18next';

// import Backend from 'i18next-http-backend';
// import LanguageDetector from 'i18next-browser-languagedetector';
// don't want to use this?
// have a look at the Quick start guide 
// for passing in lng and translations on init

export function changeLanguage(lng) {
    i18n.changeLanguage(lng);
    localStorage.setItem('userLanguage', lng);
};

i18n
    // .use(Backend)

    // .use(LanguageDetector)
    .use(initReactI18next)
    .init({
        fallbackLng: 'en',
        debug: true,
        lng: localStorage.getItem('userLanguage') || 'en',
        interpolation: {
            escapeValue: false, // not needed for react as it escapes by default
        },
        resources: {
            en: {
                translation: {
                    // Group related translations into more specific namespaces
                    roles: {
                        admin: "Admin",
                        owner: "Owner",
                        member: "Member",
                        guest: "Guest"
                    },
                    common: {
                        greeting: "Welcome on the main page 🤗",
                        createWorkspace: "Create workspace",
                        workspaceStub: "None",
                        boards: "Boards",
                        members: "Members",
                        general: "General"
                    },
                    fields: {
                        priority: {
                            priority0: "None",
                            priority1: "High",
                            priority2: "Medium",
                            priority3: "Low",  
                        },
                        labels: {
                            required: "This field is required",
                            addTask: "Add task",
                            selPriority: "Select prioirty",
                            title: "Title",
                            desc: "Description",
                            tags: "Tags",
                            priority: "Prioirity",
                            dueDate: "Due date",
                            assignee: "Assignee",
                            name: "Name",
                            email: "Email",
                            password: "Password",
                            repeatPassword: "Repeat password"
                        },
                        actions: {
                            cancel: "Cancel",
                            save: "Save",
                            create: "Create",
                            login: "Login",
                            register: "Register",
                            logout: "Log out",
                            delete: "Delete",
                            addMembers: "Add members",
                            findUser: "Find user"
                        }
                    },
                    auth: {
                        prompts: {
                            toLogin: "Already have an account?",
                            toRegister: "Do not have any account?"
                        }
                    }
                }
            },
            uk: {
                translation: {
                    roles: {
                        admin: "Адмін",
                        owner: "Власник",
                        member: "Учасник",
                        guest: "Гість"
                    },
                    common: {
                        greeting: "Вітаємо на головній сторінці 🤗",
                        createWorkspace: "Новий робочий простір",
                        workspaceStub: "Не обрано",
                        boards: "Дошки",
                        members: "Учасники",
                        general: "Загальне"
                    },
                    fields: {
                        priority: {
                            priority0: "Немає",
                            priority1: "Високий",
                            priority2: "Середній",
                            priority3: "Низький",  
                        },
                        labels: {
                            required: "Це поле є обов'язковим",
                            addTask: "Додати завдання",
                            selPriority: "Оберіть приоритет",
                            title: "Назва",
                            desc: "Опис",
                            tags: "Tags",
                            dueDate: "Дедлайн",
                            assignee: "Виконавець",
                            priority: "Приоритет",
                            name: "Ім'я",
                            email: "Ел. пошта",
                            password: "Пароль",
                            repeatPassword: "Повторіть пароль"
                        },
                        actions: {
                            cancel: "Відмінити",
                            save: "Зберегти",
                            create: "Створити",
                            login: "Увійти",
                            register: "Зареєструватися",
                            logout: "Вийти",
                            delete: "Видалити",
                            addMembers: "Додати учасників",
                            findUser: "Знайти користувача"
                        }
                    },
                    auth: {
                        prompts: {
                            toLogin: "Вже маєте обліковий запис?",
                            toRegister: "Не маєте облікового запису?"
                        }
                    }
                }
            }
        }
    });


export default i18n;