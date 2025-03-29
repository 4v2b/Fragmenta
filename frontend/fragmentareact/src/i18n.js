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
                        login: "Log in",
                        register: "Sign up",
                        stub: "Невірний логін або пароль",
                        errors: {
                            passwordMismatch: "Passwords don't match",
                            userExists: "User with such email already exists",
                            passwordTooShort: "Password is too short",
                            passwordTooWeak: "Password is too weak",
                            passwordInvalid: "Invalid password",
                            emailInvalid: "Wrong format of email",
                            emailDoesntExist: "No user with this email",
                            lockout: "Too many unsuccessful inputs. Try again later",
                            timeLeft: "{{time}} left to next try",
                        },
                        prompts: {
                            register: "Sign Up",
                            login: "Login",
                            haveAccount: "Already have an account?",
                            noAccount: "Do not have any account?",
                            forgotPassword: "Forgot password?",
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
                            tags: "Теги",
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
                        login: "Вхід",
                        register: "Реєстрація",
                        stub: "Невірний логін або пароль",
                        errors: {
                            passwordMismatch: "Паролі не співпадають",
                            userExists: "Користувач з такою ел. поштою вже існує",
                            passwordTooShort: "Пароль закороткий",
                            passwordTooWeak: "Пароль занадто слабкий",
                            passwordInvalid: "Невірний пароль",
                            emailInvalid: "Невірний формат ел. пошти",
                            userDoesntExist: "Користувача з даною ел. поштою не існує",
                            lockout: "Забагато невдалих введень. Спробуйте пізніше",
                            timeLeft: "Залишилось {{time}} до наступної спроби",
                        },
                        prompts: {
                            login: "Увійти",
                            register: "Створіть новий",
                            haveAccount: "Вже маєте обліковий запис?",
                            noAccount: "Не маєте облікового запису?",
                            forgotPassword: "Забули пароль?",
                        }
                    }
                }
            }
        }
    });


export default i18n;