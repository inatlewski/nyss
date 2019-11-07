import { initialState } from "../../../initialState";
import * as actions from "./appConstans";
import { LOCATION_CHANGE } from "connected-react-router";

export function appReducer(state = initialState.appData, action) {
  switch (action.type) {
    case LOCATION_CHANGE:
      return {
        ...state,
        moduleError: null,
        message: null
      };

    case actions.SWITCH_STRINGS:
      return {
        ...state,
        showStringsKeys: action.status
      };

    case actions.SET_APP_READY:
      return {
        ...state,
        appReady: action.status
      };

    case actions.ROUTE_CHANGED:
      return {
        ...state,
        route: {
          url: action.url,
          path: action.path,
          params: action.params
        }
      };

    case actions.INIT_APPLICATION.SUCCESS:
      return {
        ...state,
        appReady: true
      };

    case actions.GET_USER.SUCCESS:
      return {
        ...state,
        user: action.user
          ? {
            name: action.user.name,
            email: action.user.email,
            roles: action.user.roles
          }
          : null
      }

    case actions.GET_APP_DATA.SUCCESS:
      return {
        ...state,
        contentLanguages: action.contentLanguages,
        countries: action.countries,
        isDevelopment: action.isDevelopment
      }

    case actions.OPEN_MODULE.INVOKE:
      return {
        ...state,
        siteMap: {
          path: action.path,
          parameters: {},
          breadcrumb: [],
          topMenu: [],
          sideMenu: []
        }
      }

    case actions.OPEN_MODULE.SUCCESS:
      return {
        ...state,
        siteMap: {
          path: action.path,
          parameters: action.parameters,
          breadcrumb: action.breadcrumb,
          topMenu: action.topMenu,
          sideMenu: action.sideMenu
        }
      }

    case actions.OPEN_MODULE.FAILURE:
      return {
        ...state,
        moduleError: action.message
      }

    case actions.SHOW_MESSAGE.INVOKE:
      return {
        ...state,
        message: action.message
      }

    case actions.CLOSE_MESSAGE.INVOKE:
      return {
        ...state,
        message: null
      }

    default:
      return state;
  }
};
