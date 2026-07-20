import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { Account } from '../../services/account';
import { Shared } from '../../services/shared';
import { catchError, map,take, of } from 'rxjs';

export const authorizationGuard: CanActivateFn = (route, state) => {

  const accountService = inject(Account);
  const sharedService = inject(Shared);
  const router = inject(Router);
  const jwt = accountService.getJWT();
  alert(11);
  if (!jwt) {
    sharedService.showNofication(
      false,
      'Restricted Area',
      'Please login first.'
    );

    return of(
      router.createUrlTree(['/accounts/login'], {
        queryParams: { returnUrl: state.url }
      })
    );
  }

   return accountService.refreshUser(jwt).pipe(
    map(() => true),
    catchError(() => {
      accountService.logout();

      return of(
        router.createUrlTree(['/accounts/login'], {
          queryParams: { returnUrl: state.url }
        })
      );
    })
  );
};